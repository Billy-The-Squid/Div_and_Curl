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
    public XRRayInteractor teleportRay;
    public XRRayInteractor UiRay;
    public Transform attachTransform;



    /* **************************************************************************************
     * State variables measure what your hand is doing right now.
     * *************************************************************************************/

    // HAND MODE ----------------------------------------------------------------------------
    // Which type of hand is being displayed?
    public enum HandMode { Hand, Wand }
    [SerializeField] // Not the greatest solution %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    protected HandMode _mode;
    public HandMode mode { get => _mode; set { _mode = value; handModeNeedsUpdate = true; } }
    protected bool handModeNeedsUpdate;

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
    protected bool canPull { get => _canPull; set { _canPull = value; 
            //value ? forcePuller.CanPullNow() : forcePuller.CantPullNow(); // IMPLEMENT &&&&
        } }
    /* Should be false if:
     * * Holding an object
     * * Hovering
     * * pointedAtUI
     * * attemptingTeleport (?)
     * Must be true:
     * * while pulling (hopefully doesn't need explicit set?)
     */
    /// <summary> Which object would be pulled if we pressed the grip? </summary>
    protected Grabbable willBePulled;
    /* Updated each frame if:
     * * not currently pulling (extract from forcePuller) %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
     */

    /* If the action is non-waiting, an object will be pulled if
     * canPull is true
     * willBePulled is not null
     */

    // HIGHLIGHT ----------------------------------------------------------------------------
    /// <summary> If the interact button is pressed, this should be the next object to 
    /// interact with.  
    /// </summary>
    protected Outline highlightedObject; // Type? &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    /* Should be updated each frame if either canPull or hovering are true.
     * Should be disabled (pull version only) if pointedAtUI is true // redundant
     * 
     * on Update (after calculating this), set the highlightedObject's Outline component to 
     * enabled (may not need OutlineExtension after this.)
     */

    // TELEPORT -----------------------------------------------------------------------------
    [NonSerialized]
    /// <summary> Is this hand ever able to teleport? (Requires teleport ray) </summary>
    public bool teleportEnabled;
    /* Should be set on start based on the presence of a teleport ray.
     */
    /// <summary> Is this hand currently able to teleport? </summary>
    protected bool canTeleport;
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
    /// <summary> Is there a visible UI? </summary>
    protected bool nearUI;
    /// <summary> Which UIs are Visible? </summary>
    protected List<Canvas> UIsVisible = new List<Canvas>(); // This should really be a set, not a list. 
    /// <summary> Is the player's hand pointing roughly towards a UI? </summary>
    protected bool pointedAtUI; // redundant with UiRay.enabled? &&&&&&&&&&&&&&&&&&&&&&&&&&&&
    /* Should be false if:
     * * nearUI is false
     * * currently pulling (extract from forcePuller)
     * * currently grabbing (extract from directInteractor)
     * * attemptingTeleport / teleportRay.enabled.
     */


    // READOUTS -----------------------------------------------------------------------------
    //protected bool 




    private void Start()
    {
        teleportEnabled = teleportRay == null;
    }


    private void Update()
    {
        if(handModeNeedsUpdate) { UpdateHandMode(); }

        // Are we pointed at a UI?
        UiRay.TryGetCurrent3DRaycastHit(out RaycastHit hit);
        if(hit.transform.gameObject.layer == UIBackLayer 
            || hit.transform.gameObject.layer == UIBackLayer) {
            if(!pointedAtUI) {
                pointedAtUI = true; // Make a call? &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            }
        } else if(pointedAtUI) {
            pointedAtUI = false; // Make a call? &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        }

        // Can we teleport?
        if (!teleportEnabled || pointedAtUI || (directInteractor.selectTarget != null 
            && directInteractor.selectTarget.GetComponent<Resizable>() != null ))
        {
            if(canTeleport) {
                canTeleport = false; // Make the appropriate call &&&&&&&&&&&&&&&&&&&&&&&&&&&
            }
        } else if (!canTeleport) {
            canTeleport = true; // Make the appropriate call &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        }

        // Are we attempting to teleport?
        // Is this necessary?
        attemptingTeleport = teleportRay.enabled;

        // Can we pull?
        //if (forcePuller.pulling) { // is this necessary? // IMPLEMENT &&&&&&&&&&&&&&&&&&&&&
        //    canPull = true;
        //} else
        if(hovering || directInteractor.selectTarget != null || pointedAtUI || attemptingTeleport) {
            if (canPull) { canPull = false; } // Make the appropriate call &&&&&&&&&&&&&&&&&&
        } else if (!canPull) {
            canPull = true; // Make the appropriate call &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        }

        // What's the best object to pull?
        //if(!forcePuller.pulling) // IMPLEMENT &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        {
            FindWillBePulled();
            //forcePuller.nearestGrabbable = willBePulled;
        }

        // What will be highlighted?
        if(canPull) {
            highlightedObject = willBePulled.GetComponent<Outline>();
        } else if (hovering) {
            List<XRBaseInteractable> hoverTargets = new List<XRBaseInteractable>();
            directInteractor.GetHoverTargets(hoverTargets);
            highlightedObject = hoverTargets[0].GetComponent<Outline>(); 
        }
        // Highlight it.
        if(highlightedObject != null)
        {
            highlightedObject.enabled = true;
        } // OutlineExtension is unecessary &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    }



    /// <summary>
    /// Finds the nearest object in the viewcone. Updates willBePulled
    /// </summary>
    protected void FindWillBePulled()
    {
        throw new NotImplementedException();
    }



    /// <summary>
    /// Updates the hand type:  
    /// </summary>
    protected void UpdateHandMode()
    {
        throw new System.NotImplementedException("Updating hand mode not yet implemented");
        handModeNeedsUpdate = false;
    }



    /// <summary>
    /// Adds a visible UI to the user's list.
    /// </summary>
    public void SetUIVisible(Canvas UI) {
        if (!UIsVisible.Contains(UI)) {
            UIsVisible.Add(UI);
        }
        nearUI = true;
    }

    /// <summary>
    /// Removes a UI from the user's list of visible UIs.
    /// </summary>
    public void RemoveUIVisible(Canvas UI) {
        if (UIsVisible.Contains(UI)) {
            UIsVisible.Remove(UI);
        }
        nearUI = UIsVisible.Count != 0;
    }



    // Should be bound to the SelectEnter event on the directInteractor
    public void GrabSelectEntered()
    {
        objectLastHeld = directInteractor.selectTarget;
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
