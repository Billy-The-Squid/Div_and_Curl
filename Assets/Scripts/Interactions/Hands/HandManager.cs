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
                UpdateHandMode();
            }
            _mode = value;
        } 
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
        UpdateHandMode();
        if(teleporter == null) { teleportEnabled = false; }
        pullLayerMask = (1 << grabLayer) | (1 << terrainLayer);
        pointedAtUI = true;
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
        if (canPull) {
            highlightedObject = willBePulled == null ? null : willBePulled.GetComponent<Outline>();
        }
        else if (hovering) {
            List<XRBaseInteractable> hoverTargets = new List<XRBaseInteractable>();
            directInteractor.GetHoverTargets(hoverTargets);
            highlightedObject = hoverTargets[0].GetComponent<Outline>();
        }
        else { highlightedObject = null; }

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
            if (Vector3.Angle((obj.transform.position - transform.position), attachTransform.position - transform.position) <= 30) // degrees
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
    protected void UpdateHandMode()
    {
        Debug.LogWarning("UpdateHandMode not yet implemented");
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
