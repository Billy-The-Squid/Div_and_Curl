using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandManager : MonoBehaviour
{
    /* This class mostly just monitors other things, updating its internal  variables as it 
     * sees fit. It will directly enable/ disable things, or call functions (w/ UnityEvents?)
     * from other classes, in order to produce the appropriate behavior. Other classes should 
     * NOT have a reference to this one, but likely will not work without its presence. */
    

    /* **************************************************************************************
     * Outside references.
     * *************************************************************************************/
    public XRDirectInteractor directInteractor;
    public ForcePull forcePuller;
    public TeleportManager teleporter;
    public UIRay uiRay;
    public Transform attachTransform;



    /* **************************************************************************************
     * State variables measure what your hand is doing right now.
     * *************************************************************************************/


    // HAND MODE ----------------------------------------------------------------------------
    // Which type of hand is being displayed?
    public enum HandMode { Hand, Wand }
    [SerializeField] // Not the greatest solution %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    protected HandMode _mode;
    public HandMode mode 
    { 
        get => _mode; 
        set { 
            if(_mode != value)
            {
                UpdateHandMode(value);
            }
            _mode = value;
        } 
    }

    public GameObject handPrefab;
    public GameObject wandPrefab;

    protected GameObject currentHand;

    public enum Hand { Right, Left }
    [SerializeField]
    protected Hand _hand;
    public Hand hand {
        get => _hand;
        protected set { _hand = value; }
    }

    protected struct PositionSet
    {
        public Vector3 handModelPosition;
        public Quaternion handModelRotation;

        //public Vector3 directInteractorPosition;
        public Quaternion directInteractorRotation; // necessary for collider rotation;

        public Vector3 teleportRayPosition;
        public Quaternion teleportRayRotation;

        public Vector3 UIRayPosition;
        public Quaternion UIRayRotation;

        public Vector3 readoutPosition;
        public Quaternion readoutRotation;

        public Vector3 attachPosition;
        public Quaternion attachRotation;

        public Vector3 colliderPosition;

        public Vector3 raycastDirection;
    }


    // GRAB ---------------------------------------------------------------------------------
    /// <summary> Are we grabbing anything? </summary>
    //protected bool holdingObject; // Redundant with directInteractor.selectTarget == null?
    //protected GameObject objectHeld; // Redundant with directInteractor.selectTarget
    protected XRBaseInteractable objectLastHeld { get; set; } // Type? &&&&&&&&&&&&&&&&&&&&&&
    /* Should be updated on SelectEntered for the direct interactor. 
     */
    protected bool hovering { get; set; }
    /* Updated by HoverEntered (or similar) from the direct interactor.
     */


    // PULL ---------------------------------------------------------------------------------
    // Re-assess these %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    protected bool _canPull;
    /// <summary> If we tried to pull now, would it go through? </summary>
    protected bool canPull {
        get => _canPull; 
        set {
            if (_canPull != value)
            {

            }
            _canPull = value;
        } 
    }
    /* Should be false if:
     * * Holding an object
     * * Hovering
     * * pointedAtUI
     * * attemptingTeleport (?)
     * Must be true:
     * * while pulling (hopefully doesn't need explicit set?)
     */
    protected Grabbable _willBePulled;
    /// <summary> Which object would be pulled if we pressed the grip? </summary>
    protected Grabbable willBePulled
    {
        get => _willBePulled;
        set
        {
            if(_willBePulled != value)
            {
                forcePuller.nearestGrabbable = value == null ? 
                    null : value.GetComponent<XRGrabInteractable>();
            }
            _willBePulled = value;
        }
    }
    /* Updated each frame if:
     * * canPull
     * * not currently pulling (extract from forcePuller) %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
     */
    /// <summary> Every object in the scene that can be grabbed. </summary> 
    public static Grabbable[] grabbables { get; protected set; }
    /* Should be updated each frame that willBePulled is updated. Check that not null */
    // The layers that matter for grabbing. 
    static readonly int grabLayer = 9;
    static readonly int terrainLayer = 8;
    int pullLayerMask;
    /// <summary>  The direction that objects must be in to be considered for pulling. </summary>
    protected Vector3 raycastDirection;

    /* If the action is non-waiting, an object will be pulled if willBePulled is not null
     */




    // HIGHLIGHT ----------------------------------------------------------------------------
    protected Outline _highlightedObject;
    /// <summary> If the interact button is pressed, this should be the next object to 
    /// interact with.  
    /// </summary>
    protected Outline highlightedObject // Type? &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    {
        get => _highlightedObject;
        set
        {
            if(_highlightedObject != value)
            {
                UpdateColors(value);
            }
            _highlightedObject = value;
        }
    }
    /* Should be updated each frame if either canPull or hovering are true.
     */



    // TELEPORT -----------------------------------------------------------------------------
    /// <summary> Is this hand ever able to teleport? (Requires teleport ray) </summary>
    public bool teleportEnabled;
    protected bool _canTeleport;
    /// <summary> Is this hand currently able to teleport? </summary>
    protected bool canTeleport
    {
        get => _canTeleport;
        set
        {
            if(_canTeleport != value) {
                teleporter.canTeleport = value;
            }
            _canTeleport = value;
        }
    }
    /* Should be false if:
     * * teleportEnabled is false;
     * * pointedAtUI is true;
     * * resizable is being held (OnSelectEnter/exit) (and this is right hand?)
     */
    /// <summary> Is this hand currently aiming a teleport ray? </summary>
    protected bool attemptingTeleport; // Redundant with teleportRay.enabled? or LineVisual.enabled?



    // UI RAY -------------------------------------------------------------------------------
    [SerializeField]
    public static int UILayer = 5;
    public static int UIBackLayer = 14;
    protected bool _nearUI;
    /// <summary> Is there a visible UI? </summary>
    protected bool nearUI 
    { 
        get => _nearUI;
        set { 
            if (value != _nearUI) { // If this is a true update
                if (value) { uiRay.EnableRay(); }
                else { uiRay.DisableRay(); }
            }
            _nearUI = value;
        }
    }
    /// <summary> Which UIs are Visible? </summary>
    protected List<Canvas> UIsVisible = new List<Canvas>(); // This should really be a set, not a list. 
    protected bool _pointedAtUI;
    /// <summary> Is the player's hand pointing roughly towards a UI? </summary>
    protected bool pointedAtUI 
    { 
        get => _pointedAtUI;
        set { 
            if (value != _pointedAtUI) {
                if (value) { uiRay.DrawRay(); }
                else { uiRay.StopDrawRay(); } 
            }
            _pointedAtUI = value;
        }
    }
    /* Should be false if:
     * * nearUI is false
     * * currently pulling (extract from forcePuller)
     * * currently grabbing (extract from directInteractor)
     * * attemptingTeleport / teleportRay.enabled.
     */


    // READOUTS -----------------------------------------------------------------------------
    //protected bool 





    /* To do:
     * * Readout support
     * * on internals, call public methods only if value shifts
     * * Resizables
     */













    private void Start()
    {
        UpdateHandMode(mode);
        if(teleporter == null) { teleportEnabled = false; }
        pullLayerMask = (1 << grabLayer) | (1 << terrainLayer);
        pointedAtUI = true;
        if(hand == Hand.Left)
        {
            // Do stuff &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        }
    }


    private void Update()
    {
        // Are we pointed at a UI?
        if(uiRay.ray.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.transform.gameObject.layer == UIBackLayer
            || hit.transform.gameObject.layer == UIBackLayer) {
                pointedAtUI = true;
            }
            else {
                pointedAtUI = false;
            }
        } 
        else {
            pointedAtUI = false;
        }


        // Can we teleport?
        if (!teleportEnabled || pointedAtUI || (directInteractor.selectTarget != null
            && directInteractor.selectTarget.GetComponent<Resizable>() != null))
        {
            //if(canTeleport) 
            {
                canTeleport = false;
            }
        }
        else //if (!canTeleport) 
        {
            canTeleport = true;
        }

        // Are we attempting to teleport?
        // Is this necessary?
        if(teleporter != null) {
            attemptingTeleport = teleporter.rayInteractor.enabled;
        }


        // Can we pull?
        if (forcePuller.pulling)
        { // is this necessary?
            canPull = true;
        }
        else if (hovering || directInteractor.selectTarget != null || pointedAtUI || attemptingTeleport) {
            canPull = false; // Make the appropriate call &&&&&&&&&&&&&&&&&&
        }
        else {
            canPull = true; // Make the appropriate call &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        }


        if (canPull) {
            // What's the best object to pull?
            if (!forcePuller.pulling)
            {
                FindWillBePulled();
            }
        }
        else {
            willBePulled = null;
        }

        // What will be highlighted?
        //if (canPull) 
        //{
        //    highlightedObject = willBePulled == null ? null : willBePulled.GetComponent<Outline>();
        //}
        //else if (hovering) {
        //    List<XRBaseInteractable> hoverTargets = new List<XRBaseInteractable>();
        //    directInteractor.GetHoverTargets(hoverTargets);
        //    highlightedObject = hoverTargets[0].GetComponent<Outline>();
        //}
        //else { highlightedObject = null; }

        // Debug Code
        Debug.Log(name + " highlighted object: " + highlightedObject?.name);

        //directInteractor.allowHover = !forcePuller.pulling; // Doesn't do anything? 
    }



    /// <summary>
    /// Finds the nearest object in the viewcone. Updates willBePulled
    /// </summary>
    protected void FindWillBePulled()
    {
        grabbables = FindObjectsOfType<Grabbable>();

        Grabbable bestYet = null;
        float dist = float.MaxValue;

        foreach (Grabbable obj in grabbables)
        {
            // Is it in the general direction?
            if (Vector3.Angle((obj.transform.position - transform.position), raycastDirection) <= 30) // degrees
            {
                // Do we have line-of-sight?
                if (Physics.Raycast(transform.position, obj.transform.position - transform.position, out RaycastHit hit, 40f, pullLayerMask))
                {
                    if (hit.transform.Equals(obj.transform))
                    {
                        // I'm not holding it already, right?
                        if((!obj.GetComponent<XRGrabInteractable>().isSelected || !(obj.GetComponent<XRGrabInteractable>().selectingInteractor is XRDirectInteractor)))
                        {
                            // Is it the closest?
                            if (hit.distance < dist)
                            {
                                bestYet = hit.transform.GetComponent<Grabbable>();
                                dist = hit.distance;
                            }
                        }
                    }
                }
            }
        }

        willBePulled = bestYet;
    }

    /// <summary> Updates the color of the current and most recent willBeGrabbed </summary>
    private void UpdateColors(Outline next)
    {
        try
        {
            _highlightedObject.GetComponent<Outline>().enabled = false;
        }
        catch (NullReferenceException)
        {
            if (_highlightedObject != null)
            {
                Debug.LogWarning("Last grabbable " + _highlightedObject.name + " does not have outline component.");
            }
        }
        catch (MissingReferenceException)
        {
            // Object has been destroyed.
        }

        try
        {
            next.GetComponent<Outline>().enabled = true;
        }
        catch (NullReferenceException)
        {
            if (next != null)
            {
                Debug.LogWarning("Grabbable " + next.name + " does not have outline component.");
            }
        }
    }



    /// <summary>
    /// Updates the hand type:  
    /// </summary>
    protected void UpdateHandMode(HandMode newMode)
    {
        PositionSet posSet = GetPositions(newMode);

        // Set the positions of other things.
        // Direct interactor
        directInteractor.transform.localRotation = posSet.directInteractorRotation;
        // attach point
        attachTransform.localPosition = posSet.attachPosition;
        attachTransform.localRotation = posSet.attachRotation;
        // teleport ray (if != null)
        if (teleporter != null)
        {
            teleporter.rayInteractor.transform.localPosition = posSet.teleportRayPosition;
            teleporter.rayInteractor.transform.localRotation = posSet.teleportRayRotation;
        }
        // uiRay
        uiRay.ray.transform.localPosition = posSet.UIRayPosition;
        uiRay.ray.transform.localRotation = posSet.UIRayRotation;

        // Set the collider type/position
        Debug.LogWarning("Collider unset");

        // Raycast direction
        raycastDirection = posSet.raycastDirection;

        // Move the readout display. // Might require parenting first.
        Debug.LogWarning("readout unset");


        // Hand mode ------------------------------------------------------------------------
        if (newMode == HandMode.Hand)
        {
            // Make sure we have the prefab.
            if(handPrefab == null) {
                throw new Exception("Hand prefab missing");
            }
            // Destroy the previous prefab.
            if(currentHand != null) {
                Destroy(currentHand.gameObject);
            }

            // Make the hands and set them.
            currentHand = Instantiate(handPrefab);
            currentHand.transform.parent = transform;
            currentHand.transform.localPosition = posSet.handModelPosition;
            currentHand.transform.localRotation = posSet.handModelRotation;


            // Determine how the hand disappears
            Debug.LogWarning("hand disppearance unset");

        }

        // Wand mode ------------------------------------------------------------------------
        else
        {
            if(wandPrefab == null) {
                throw new Exception("Wand prefab missing");
            }
            if(currentHand != null) {
                Destroy(currentHand.gameObject);
            }

            currentHand = Instantiate(wandPrefab);
            currentHand.transform.parent = transform;
            currentHand.transform.localPosition = posSet.handModelPosition;
            currentHand.transform.localRotation = posSet.handModelRotation;

            Debug.LogWarning("Wand mode unimplemented");
        }

        Debug.LogWarning("UpdateHandMode not yet implemented");
    }

    protected PositionSet GetPositions(HandMode mode)
    {
        {
            //protected struct PositionSet
            //{
            //    public Vector3 handModelPosition;
            //    public Quaternion handModelRotation;

            //    public Quaternion directInteractorRotation;

            //    public Vector3 teleportRayPosition;
            //    public Quaternion teleportRayRotation;

            //    public Vector3 UIRayPosition;
            //    public Quaternion UIRayRotation;

            //    public Vector3 readoutPosition;
            //    public Quaternion readoutRotation;

            //    public Vector3 attachPosition;
            //    public Quaternion attachRotation;

            //    public Vector3 colliderPosition;
            //}
        }

        PositionSet positionSet = new PositionSet();
        // Set positions for the right hand.
        if(mode == HandMode.Hand)
        {
            positionSet.handModelPosition = new Vector3(0.001f, 0.001f, -0.035f); // Revise these! &&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            positionSet.handModelRotation = Quaternion.Euler(0, 0, -90);
            positionSet.directInteractorRotation = Quaternion.Euler(0, 0, 0);
            positionSet.teleportRayPosition = new Vector3(0.05f, -0.005f, 0.1f);
            positionSet.teleportRayRotation = Quaternion.Euler(0, 0, 0);
            positionSet.UIRayPosition = new Vector3(0.05f, -0.005f, 0.1f);
            positionSet.UIRayRotation = Quaternion.Euler(0, 0, 0);
            positionSet.readoutPosition = new Vector3(-0.02f, -0.03f, 0.015f);
            positionSet.readoutRotation = Quaternion.Euler(20, 45, 90);
            positionSet.attachPosition = new Vector3(0f, 0f, 0f);
            positionSet.attachRotation = Quaternion.Euler(0, 0, 0);
            Debug.LogWarning("Collider data not set");
            positionSet.raycastDirection = new Vector3(0, 0, 1);
        }
        else
        {
            positionSet.handModelPosition = new Vector3(0.01f, -0.015f, -0.05f);
            positionSet.handModelRotation = Quaternion.Euler(-45, 0, 0);
            positionSet.directInteractorRotation = Quaternion.Euler(45, 0, 0);
            positionSet.teleportRayPosition = new Vector3(0.01f, 0.08f, 0.09f);
            positionSet.teleportRayRotation = Quaternion.Euler(0, 0, 0);
            positionSet.UIRayPosition = new Vector3(0.01f, 0.08f, 0.09f);
            positionSet.UIRayRotation= Quaternion.Euler(0, 0, 0);
            positionSet.readoutPosition = new Vector3(0.01f, 0.15f, 0f);
            positionSet.readoutRotation = Quaternion.Euler(0, 0, 0);
            positionSet.attachPosition = new Vector3(0.01f, 0.3f, 0.007f);
            positionSet.attachRotation = Quaternion.Euler(-45, 0, 0);
            Debug.LogWarning("Collider data not set");
            positionSet.raycastDirection = new Vector3(0, 1, 1);
        }

        // Set positions for the left hand.
        if (hand == Hand.Left)
        {
            positionSet.handModelPosition = MirrorPosition(positionSet.handModelPosition);
            positionSet.handModelRotation = MirrorRotation(positionSet.handModelRotation);
            positionSet.directInteractorRotation = MirrorRotation(positionSet.directInteractorRotation);
            positionSet.teleportRayPosition = MirrorPosition(positionSet.teleportRayPosition);
            positionSet.teleportRayRotation = MirrorRotation(positionSet.teleportRayRotation);
            positionSet.UIRayPosition = MirrorPosition(positionSet.UIRayPosition);
            positionSet.UIRayRotation = MirrorRotation(positionSet.UIRayRotation);
            positionSet.readoutPosition = MirrorPosition(positionSet.readoutPosition);
            positionSet.readoutRotation = MirrorRotation(positionSet.readoutRotation);
            positionSet.attachPosition = MirrorPosition(positionSet.attachPosition);
            positionSet.attachRotation = MirrorRotation(positionSet.attachRotation);
            Debug.LogWarning("Collider data not set");
        }

        Vector3 MirrorPosition(Vector3 position)
        {
            Vector3 ret = position;
            ret.x *= -1;
            return ret;
        }
        Quaternion MirrorRotation(Quaternion rotation)
        {
            Vector3 retEulers = rotation.eulerAngles;
            retEulers.y *= -1;
            retEulers.z *= -1;
            return Quaternion.Euler(retEulers);
        }

        return positionSet;
    }




    /// <summary>
    /// Adds a visible UI to the user's list.
    /// </summary>
    public void SetUIVisible(Canvas UI) {
        if (!UIsVisible.Contains(UI)) {
            UIsVisible.Add(UI);
        }
        nearUI = true; // Call the appropriate function &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    }

    /// <summary>
    /// Removes a UI from the user's list of visible UIs.
    /// </summary>
    public void RemoveUIVisible(Canvas UI) {
        if (UIsVisible.Contains(UI)) {
            UIsVisible.Remove(UI);
        }
        nearUI = UIsVisible.Count != 0; // Call the appropriate function &&&&&&&&&&&&&&&&&&&&
    }



    // Should be bound to the SelectEnter event on the directInteractor
    public void GrabSelectEntered()
    {
        objectLastHeld = directInteractor.selectTarget;
        willBePulled = null;
    }

    // Should be bound to the SelectExitevent on the directInteractor
    public void GrabSelectExited()
    {

    }

    // Should be bound to the HoverEnter event on the directInteractor
    public void GrabHoverEntered()
    {
        hovering = true;
    }

    // Should be bound to the HoverExit event on the directInteractor
    public void GrabHoverExited()
    {
        hovering = false;
    }


}
