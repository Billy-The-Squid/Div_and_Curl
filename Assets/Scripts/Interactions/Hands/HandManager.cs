using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

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
    public HandHeldUI readout;
    public Collider handCollider;
    public Collider wandCollider;
    public MovementManager movementManager;
    protected DetectorSelector detectorStation;
    public HandManager otherHand;
    public LineRenderer highlightRay;

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
                _mode = value;
                isVisible = isVisible; // Force a reevaluation of the things that depend on this. 
            }
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

        //public Vector3 teleportRayPosition;
        //public Quaternion teleportRayRotation;

        //public Vector3 UIRayPosition;
        //public Quaternion UIRayRotation;

        public Vector3 rayPosition;
        public Quaternion rayRotation;

        public Vector3 attachPosition;
        public Quaternion attachRotation;

        public Vector3 colliderPosition;

        public Vector3 raycastDirection;
    }
    protected bool _isVisible;
    protected bool isVisible
    {
        get => _isVisible;
        set
        {
            if(mode == HandMode.Hand)
            {
                currentHand.GetComponent<HandMotion>().SetVisible(value);
            }
            else
            {
                attachTransform.GetComponent<MeshRenderer>().enabled = value; // && !pointedAtUI;
            }
            _isVisible = value;
        }
    }



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
                forcePuller.nearestGrabbable = value;
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
    protected Grabbable _highlightedObject;
    /// <summary> If the interact button is pressed, this should be the next object to 
    /// interact with.  
    /// </summary>
    protected Grabbable highlightedObject
    {
        get => _highlightedObject;
        set
        {
            if(_highlightedObject != value)
            {
                if(highlightStyle == HighlightStyle.Outline)
                {
                    UpdateColors(value);
                }
                else if(highlightRay != null)
                {
                    if(value != null && value == willBePulled)
                    {
                        highlightRay.enabled = true;
                    }
                    else { highlightRay.enabled = false; }
                }
            }
            _highlightedObject = value;
        }
    }
    /* Should be updated each frame if either canPull or hovering are true.
     */
    protected Grabbable.Highlight _highlightMode;
    protected Grabbable.Highlight highlightMode
    {
        get => _highlightMode;
        set
        {
            if(_highlightMode != value)
            {
                _highlightMode = value;
                UpdateColors(highlightedObject);
                //Debug.Log("Changed highlight mode to " + value);
            }
        }
    }
    public enum HighlightStyle { Outline, Ray }
    public HighlightStyle highlightStyle = HighlightStyle.Ray;



    // MOVEMENT -----------------------------------------------------------------------------



    // TELEPORT -----------------------------------------------------------------------------
    /// <summary> Is this hand ever able to teleport? (Requires teleport ray) </summary>
    public bool teleportEnabled;
    /* Should be false if:
     * * this hand does not have a teleporter
     * * teleporter.enabled is false
     * * teleporter.gameObject.isActive is false
     * * movement mode is set to smooth
     * Update on:
     * * Start
     * * Changing movement mode
     */
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
    protected int UILayermask;
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
     * 
     * Should be true otherwise based on raycast
     */


    // READOUTS -----------------------------------------------------------------------------
    protected InputAction selectAction;
    protected Grabbable objectLastHeld { get; set; } // Type? &&&&&&&&&&&&&&&&&&&&&&
    /* Should be updated on SelectEntered for the direct interactor. 
     */
    protected bool hovering { get; set; }
    /* Updated by HoverEntered (or similar) from the direct interactor.
     */




    // RESIZING -----------------------------------------------------------------------------
    //protected Vector3 defaultAttachPosition;
    public InputActionAsset actionAsset;
    public InputAction resizeAction { get; protected set; }
    public Resizable resizable { get; protected set; }
    /* Updated each frame.
     * * If we're holding one, this is it. 
     * * If not, it defaults to whatever is in the other hand,
     *      then to what's on the pedestal. 
     */




    /* To do:
     *  * Respond if other hand also has detector
     *  * Respond on resize
     * * on internals, call public methods only if value shifts
     * * Resizables (only one at a time)
     * * Make a local bool wrapper for teleport.enabled, and have it call CheckTeleportEnabled.
     */













    private void Start()
    {
        UpdateHandMode(mode);
        CheckTeleportEnabled();
        pullLayerMask = (1 << grabLayer) | (1 << terrainLayer);
        UILayermask = (1 << UILayer) | (1 << UIBackLayer);
        pointedAtUI = true;
        nearUI = true; // Find a better way to do this &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&

        highlightRay.enabled = false;

        if(detectorStation == null) {
            detectorStation = FindObjectOfType<DetectorSelector>();
        }

        // Find the resizable and select actions.
        if(actionAsset == null) {
            Debug.LogError("HandManager was not given an action asset");
        }
        else
        {
            InputActionMap map = actionAsset.FindActionMap("XRI " + hand + "Hand");
            if(map == null)
            {
                Debug.LogError("Cannot find ActionMap: XRI " + hand + "Hand");
            }
            else
            {
                resizeAction = map.FindAction("Resize Object");
                selectAction = map.FindAction("Select");
            }
        }

        if(selectAction != null)
        {
            selectAction.started += ResetReadout;
        }
        else
        {
            Debug.LogError("Select action is null");
        }

        ResetReadout(new InputAction.CallbackContext());
    }


    private void Update()
    {
        // Update the current resizable
        UpdateResizable();
        if(resizable != null && resizeAction != null && (resizeAction.phase == InputActionPhase.Performed || resizeAction.phase == InputActionPhase.Started))
        {
            Resizable.SizeChange change = new Resizable.SizeChange();
            float current = resizeAction.ReadValue<Vector2>().y;
            if (current > 0f)
            {
                change = resizable.SizeUp();
            }
            else if (current < 0f)
            {
                change = resizable.SizeDown();
            }
            
            if(change.isValid)
            {
                if(mode == HandMode.Wand)
                {
                    Debug.LogWarning("Have not adopted smart attach point shifting yet");
                }
            }
        }

        {
            //if (hand != Hand.Right)
            //{
            //    Debug.Log("nearUI: " + nearUI);
            //    //Debug.Log("not pulling: " + !forcePuller.pulling);
            //    //Debug.Log("Nothing selected: " + (directInteractor.selectTarget == null));
            //    //Debug.Log("Not attempting a teleport: " + !attemptingTeleport);
            //    ////Debug.Log("Select target: " + directInteractor.selectTarget.name);
            //    Debug.Log("pointed at UI? " + pointedAtUI);
            //    Debug.Log("UIRay.ray enabled? " + uiRay.ray.enabled);
            //    Debug.Log("Raycast attempt: " + uiRay.ray.TryGetCurrent3DRaycastHit(out RaycastHit thing));
            //    //Debug.Log("Is thing transform null? " + (thing.transform == null));
            //    //Ray ray = new Ray(uiRay.ray.transform.position, uiRay.ray.transform.forward);
            //    //Debug.DrawRay(uiRay.ray.transform.position, uiRay.ray.transform.forward);
            //    //Debug.Log("Less dumb raycast: " + Physics.Raycast(ray, out RaycastHit thing2, 5, UILayermask)); //, uiRay.ray.interactionLayerMask));
            //    //if(thing2.transform != null) Debug.Log("Thing2 name: " + thing2.transform.name);
            //}

            //Debug.Log("TeleportEnabled: " + teleportEnabled);
        }

        // Are we pointed at a UI?
        if (nearUI && !forcePuller.pulling && (directInteractor.selectTarget == null) 
            && !attemptingTeleport && uiRay.ray.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            //if (hand != Hand.Right) { Debug.Log("Raycast hit: " + hit.transform.name + "\nLayer: " + hit.transform.gameObject.layer); }
            if (hit.transform.gameObject.layer == UILayer
            || hit.transform.gameObject.layer == UIBackLayer) {
                pointedAtUI = true;
                //Debug.Log("Made it through raycast");
            }
            else {
                pointedAtUI = false;
                //Debug.Log("Did not make it through raycast.");
            }
        } 
        else {
            pointedAtUI = false;
        }

        
        // Can we teleport?
        if (!teleportEnabled || pointedAtUI || (directInteractor.selectTarget != null
            && directInteractor.selectTarget.GetComponent<Resizable>() != null))
        {
            {
                canTeleport = false;
            }
        }
        else
        {
            canTeleport = true;
        }

        // Are we attempting to teleport?
        // Is this necessary?
        if(teleporter != null) {
            attemptingTeleport = teleporter.rayInteractor.enabled;
        }

        // Is the directInteractor hovering something?
        List<XRBaseInteractable> hoverTargets = new List<XRBaseInteractable>();
        directInteractor.GetHoverTargets(hoverTargets);
        hovering = hoverTargets.Count > 0;

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

        // What's the best object to pull?
        if (canPull) {
            if (!forcePuller.pulling)
            {
                FindWillBePulled();
            }
        }
        else {
            willBePulled = null;
        }

        FindObjectToHighlight();

        //directInteractor.allowHover = !forcePuller.pulling; // Doesn't do anything? 

        // Make the hand disappear if we're holding something that's not a UI.
        isVisible = (directInteractor.selectTarget == null || directInteractor.selectTarget.GetComponent<HandHeldUI>() != null);

        if (mode == HandMode.Hand)
        {
            currentHand.GetComponent<HandMotion>().isPointing = attemptingTeleport;// || pointedAtUI || highlightRay.enabled;
        }
    }


    /// <summary>
    /// Update the value of resizable
    /// </summary>
    protected void UpdateResizable() {
        if(directInteractor.selectTarget != null && directInteractor.selectTarget.GetComponent<Resizable>() != null) {
            resizable = directInteractor.selectTarget.GetComponent<Resizable>();
        }
        else if(otherHand.directInteractor.selectTarget != null && otherHand.directInteractor.selectTarget.GetComponent<Resizable>() != null) {
            resizable = otherHand.directInteractor.selectTarget.GetComponent<Resizable>();
        }
        else if(detectorStation.instantiated != null && detectorStation.instantiated.GetComponent<Resizable>() != null) {
            resizable = detectorStation.instantiated.GetComponent<Resizable>();
        }
        else {
            resizable = null;
        }
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
            //Debug.Log(name + " examining object: " + obj.name + "\nDisplay");
            // Is it in the general direction?
            if (Vector3.Angle((obj.transform.position - transform.position), transform.TransformVector(raycastDirection)) <= 30) // degrees
            {

                // Do we have line-of-sight?
                if (Physics.Raycast(transform.position, obj.transform.position - transform.position, out RaycastHit hit, 40f, pullLayerMask))
                {
                    if (hit.transform.Equals(obj.transform))
                    {
                        //if(obj.GetComponent<XRGrabInteractable>() == null)
                        //{
                        //    Debug.LogWarning(obj.name + " is missing an XRGrabInteractable component");
                        //}
                        // I'm not holding it already, right?
                        if((!obj.isSelected || !(obj.selectingInteractor is XRDirectInteractor)))
                        {
                            // Is it the closest?
                            if (hit.distance < dist)
                            {
                                bestYet = obj;
                                dist = hit.distance;
                            }
                        }
                    }
                }
            }
        }

        willBePulled = bestYet;
    }


    // MAKE HIGHLIGHT RAYS BETTER &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    /// <summary>
    /// Sets highlightedObject
    /// </summary>
    protected void FindObjectToHighlight()
    {
        /* Priority:
         * If selected, nothing is highlighted
         * If hovering, hovering object is highlighted
         * If willBePulled is not null, it's highlighted.
         */
        //if(highlightRay != null)
        //{
        //    highlightRay.enabled = false; // This isn't the best way to do this. Instead, tie a check into highlightedObject's set. &&&&&&&&&&&&&&&&&&&&&&&
        //}

        if (directInteractor.selectTarget != null) {
            highlightMode = Grabbable.Highlight.Normal;
            highlightedObject = null;
        }
        else if(hovering) {
            //Debug.Log("Still hovering");
            //Debug.Log("Highlighted object: " + (highlightedObject == null ? 0 : highlightedObject.GetInstanceID()));
            List<XRBaseInteractable> hoverTargets = new List<XRBaseInteractable>();
            directInteractor.GetHoverTargets(hoverTargets);
            hoverTargets.Sort(new Nearest(attachTransform));
            highlightMode = Grabbable.Highlight.Normal;
            if(hoverTargets[0] is Grabbable)
            {
                highlightedObject = ((Grabbable)hoverTargets[0]);
            }
        } 
        else if (canPull) {
            //Debug.Log("Can pull");
            //Debug.Log("Highlighted object: " + (highlightedObject == null ? 0 : highlightedObject.GetInstanceID()));
            if (willBePulled != null && willBePulled == detectorStation.instantiated && detectorStation.isFirst[detectorStation.available[detectorStation.current]])
            {
                highlightMode = Grabbable.Highlight.Invalid;
                highlightedObject = willBePulled;
                willBePulled = null;
                //Debug.LogWarning("Can't pull that");
            }
            else
            {
                highlightMode = Grabbable.Highlight.Normal;
                highlightedObject = willBePulled;
                //if(highlightRay != null && highlightedObject != null)
                //{
                //    highlightRay.enabled = true;
                //    highlightRay.SetPosition(0, highlightRay.transform.position);
                //    highlightRay.SetPosition(1, highlightedObject.transform.position);
                //}
            }
        }
        else
        {
            highlightedObject = null;
        }

        if(highlightedObject != null)
        {
            highlightRay.SetPosition(0, highlightRay.transform.position);
            highlightRay.SetPosition(1, highlightedObject.transform.position);
        }
    }

    /// <summary> Updates the color of the current and most recent willBeGrabbed </summary>
    private void UpdateColors(Grabbable next)
    {
        //Debug.Log("UpdateColors called: going from " + (_highlightedObject ? _highlightedObject.name : "None") + " to " + (next ? next.name : "None"));
        if(_highlightedObject != null && _highlightedObject != next)
        {
            try
            {
                _highlightedObject.RemoveHighlighter(this.gameObject);
                //Debug.Log("Removed highlight");
            }
            catch (MissingReferenceException)
            {
                // Object has been destroyed.
            }
        }
        

        try
        {
            //Debug.Log("Adding highlights: " + highlightMode);
            next.AddHighlighter(this.gameObject, highlightMode);
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
    /// Updates the hand type 
    /// </summary>
    protected void UpdateHandMode(HandMode newMode)
    {
        PositionSet posSet = GetPositions(newMode);

        // Set the positions of other things.
        // Direct interactor
        directInteractor.transform.localRotation = posSet.directInteractorRotation;
        // attach point
        attachTransform.localPosition = posSet.attachPosition;
        attachTransform.localRotation = posSet.attachRotation; // Make sure this interacts properly with Grabber class
        // teleport ray (if != null)
        if (teleporter != null)
        {
            teleporter.rayInteractor.transform.localPosition = posSet.rayPosition;
            teleporter.rayInteractor.transform.localRotation = posSet.rayRotation;
        }
        // uiRay
        uiRay.ray.transform.localPosition = posSet.rayPosition;
        uiRay.ray.transform.localRotation = posSet.rayRotation;

        highlightRay.transform.localPosition = posSet.rayPosition;
        highlightRay.transform.localRotation = posSet.rayRotation;

        // Raycast direction
        raycastDirection = posSet.raycastDirection;

        MeshRenderer attachRender = attachTransform.GetComponent<MeshRenderer>();

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

            handCollider.enabled = true;
            wandCollider.enabled = false;

            // Determine how the hand disappears
            currentHand.GetComponent<HandMotion>().SetVisible(isVisible);

            if (attachTransform.GetComponent<MeshRenderer>() != null) { attachRender.enabled = false; }
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

            handCollider.enabled = false;
            wandCollider.enabled = true;

            if (attachTransform.GetComponent<MeshRenderer>() != null) { attachRender.enabled = true; }
        }
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
            positionSet.rayPosition = new Vector3(0.05f, -0.005f, 0.1f);
            positionSet.rayRotation = Quaternion.Euler(0, 0, 0);
            positionSet.attachPosition = new Vector3(0.03f, -0.035f, -0.04f);
            positionSet.attachRotation = Quaternion.Euler(0, 0, 0);
            //Debug.LogWarning("Collider data not set");
            positionSet.raycastDirection = new Vector3(0, 0, 1);
        }
        else
        {
            positionSet.handModelPosition = new Vector3(0.01f, -0.015f, -0.05f);
            positionSet.handModelRotation = Quaternion.Euler(-45, 0, 0);
            positionSet.directInteractorRotation = Quaternion.Euler(45, 0, 0);
            positionSet.attachPosition = new Vector3(0.01f, 0.3f, -0.02f);
            //positionSet.rayPosition = new Vector3(0.01f, 0.08f, 0.09f);
            positionSet.rayPosition = positionSet.directInteractorRotation * positionSet.attachPosition;
            positionSet.rayRotation = Quaternion.Euler(0, 0, 0);
            positionSet.attachRotation = Quaternion.Euler(0.01f, 0.3f, 0.007f);
            //Debug.LogWarning("Collider data not set");
            positionSet.raycastDirection = new Vector3(0, 1, 1);
        }

        // Set positions for the left hand.
        if (hand == Hand.Left)
        {
            positionSet.handModelPosition = MirrorPosition(positionSet.handModelPosition);
            positionSet.handModelRotation = MirrorRotation(positionSet.handModelRotation);
            positionSet.directInteractorRotation = MirrorRotation(positionSet.directInteractorRotation);
            positionSet.rayPosition = MirrorPosition(positionSet.rayPosition);
            positionSet.rayRotation = MirrorRotation(positionSet.rayRotation);
            positionSet.attachPosition = MirrorPosition(positionSet.attachPosition);
            positionSet.attachRotation = MirrorRotation(positionSet.attachRotation);
            //Debug.LogWarning("Collider data not set");
            positionSet.raycastDirection = MirrorPosition(positionSet.raycastDirection);
        }
        return positionSet;
    }


    protected Vector3 MirrorPosition(Vector3 position)
    {
        Vector3 ret = position;
        ret.x *= -1;
        return ret;
    }
    protected Quaternion MirrorRotation(Quaternion rotation)
    {
        Vector3 retEulers = rotation.eulerAngles;
        retEulers.y *= -1;
        retEulers.z *= -1;
        return Quaternion.Euler(retEulers);
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
        if(directInteractor.selectTarget is Grabbable)
        {
            objectLastHeld = (Grabbable)directInteractor.selectTarget;
        }
        willBePulled = null;

        if(directInteractor.selectTarget is FieldDetector)
        {
            readout.detector = (FieldDetector)directInteractor.selectTarget;
        }
    }

    // Should be bound to the SelectExitevent on the directInteractor
    public void GrabSelectExited()
    {

    }

    public void ResetReadout(InputAction.CallbackContext context)
    {
        //readout.detector = null;

        StartCoroutine(WaitToCheck());

        IEnumerator WaitToCheck()
        {
            yield return new WaitForSeconds(0.01f);
            if(directInteractor.selectTarget == null)
            {
                readout.detector = null;
            }
        }
    }

    //protected IEnumerator

    public void ChangeHandMode(Int32 mode)
    {
        this.mode = (HandMode)mode;
    }

    protected class Nearest : IComparer<XRBaseInteractable>
    {
        protected Transform origin;

        public Nearest(Transform origin)
        {
            this.origin = origin;
        }

        public int Compare(XRBaseInteractable x, XRBaseInteractable y)
        {
            return Mathf.CeilToInt((x.transform.position - origin.position).magnitude - (y.transform.position - origin.position).magnitude);
        }
    }

    public void ChangeMovementMode()
    {
        if(movementManager.movementMode == MovementManager.MovementMode.Smooth) {
            if(teleporter != null)
            {
                teleporter.enabled = false;
            }
        }
        else // teleport mode
        {
            if(teleporter != null)
            {
                teleporter.enabled = true;
            }
        }
        CheckTeleportEnabled();
    }

    /// <summary>
    /// Sets the value of teleportEnabled.
    /// </summary>
    protected void CheckTeleportEnabled()
    {
        teleportEnabled = teleporter != null && teleporter.enabled && teleporter.gameObject.activeInHierarchy;
        // use teleporter.enabled to disable teleportation in-game (for instance, changing modes).
        // Use teleporter.SetActive (or disable the entire gameobject) for a disable that persists across modes.
    }
}
