using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


// Needs documentation
public class ForcePull : MonoBehaviour
{
    // More frequent updates needed. %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    /// <summary>
    /// Every object in the scene that can be grabbed. 
    /// 
    /// Updated at Start. 
    /// </summary> 
    public static XRGrabInteractable[] grabbables { get; protected set; }
    
    /// <summary>
    /// The object that either will be pulled or is being pulled.
    /// </summary>
    public XRGrabInteractable nearestGrabbable { get; protected set; }
    /// <summary>
    /// Last frame's nearestGrabbable.
    /// </summary>
    protected XRGrabInteractable lastGrabbable;

    //// This isn't used properly %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    ///// <summary>
    ///// Should be false if the user is currently pulling something or holding something. 
    ///// </summary>
    //bool handBusy = false;

    public enum HandState { Empty, Holding, Pulling };
    public HandState busy = HandState.Empty;

    // The layers that matter for grabbing. 
    static readonly int grabLayer = 9;
    static readonly int terrainLayer = 8;
    int layerMask;

    /// <summary>
    /// The input actions asset that the grab actions will be read from. 
    /// </summary>
    [SerializeField]
    InputActionAsset actionAsset;

    /// <summary>
    /// The grab action.
    /// </summary>
    protected InputAction grab;

    protected enum Hands { Right, Left }
    /// <summary>
    /// Which hand is this?
    /// </summary>
    [SerializeField]
    Hands hand;
    protected string[] handNames = { "Right", "Left" };

    /// <summary>
    /// The speed at which the grabbable is pulled towards you.
    /// </summary>
    [SerializeField]
    float pullSpeed;

    /// <summary>
    /// The XRDirect Interactor attached to this hand. 
    /// </summary>
    public XRDirectInteractor directInteractor;

    /// <summary>
    /// The transform towards which to pull the object.
    /// </summary>
    public Transform attachAnchorTransform;
    // Should be able to find this automatically






    private void Start()
    {
        if(directInteractor == null)
        {
            directInteractor = GetComponent<XRDirectInteractor>();
        }

        // Initialize the set of possible grabbables
        // Do this better. &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        grabbables = FindObjectsOfType<XRGrabInteractable>();
        nearestGrabbable = null;
        lastGrabbable = null;
        layerMask = (1 << grabLayer) | (1 << terrainLayer);

        //handBusy = false;
        busy = HandState.Empty;

        // Find the input action. 
        string handName = handNames[(int)hand];
        grab = actionAsset.FindActionMap("XRI " + handName + "Hand").FindAction("Select");
    }

    // Update is called once per frame
    void Update()
    {
        // Find a better way to do this. 
        grabbables = FindObjectsOfType<XRGrabInteractable>();

        UpdatePull();

        // Search if your hands aren't occupied. 
        if (busy == HandState.Empty) {
            SearchForGrabbables();
        }

        // Turn off highlight if we're already holding the object
        if(busy == HandState.Holding) {
            nearestGrabbable = null;
            UpdateColors();
        }

        // Pull if you're pulling
        if(busy == HandState.Pulling) {
            if(nearestGrabbable != null && !nearestGrabbable.isSelected) {
                nearestGrabbable.GetComponent<Rigidbody>().MovePosition(Vector3.MoveTowards(
                    nearestGrabbable.transform.position, attachAnchorTransform.position, pullSpeed * Time.deltaTime));
            }
        }
    }



    /// <summary>
    /// Sets the value of busy. 
    /// </summary>
    private void UpdatePull()
    { // This could also be done with interactiion events
        if(grab.phase == InputActionPhase.Waiting)
        {
            busy = HandState.Empty;
        } else if (directInteractor.isSelectActive && directInteractor.selectTarget != null) // verify that this works as intended.
        {
            busy = HandState.Holding;
        } else // I think this is right?
        {
            busy = HandState.Pulling;
        }
    }

    /// <summary>
    /// Updates the value of nearestGrabbable
    /// </summary>
    private void SearchForGrabbables()
    {
        nearestGrabbable = null;
        float dist = float.MaxValue;
        
        foreach(XRGrabInteractable obj in grabbables)
        {
            // Is it in the general direction?
            if(Vector3.Angle((obj.transform.position - transform.position), transform.forward) <= 30) // degrees
            {
                // Do we have line-of-sight?

                if (Physics.Raycast(transform.position, obj.transform.position - transform.position, out RaycastHit hit, 40f, layerMask))
                {
                    if (hit.transform.Equals(obj.transform))
                    {
                        // Is it the closest?
                        if (nearestGrabbable == null)
                        {
                            nearestGrabbable = hit.transform.GetComponent<XRGrabInteractable>();
                            dist = hit.distance;
                        }
                        else
                        {
                            if (hit.distance < dist)
                            {
                                nearestGrabbable = hit.transform.GetComponent<XRGrabInteractable>();
                                dist = hit.distance;
                            }
                        }
                    }
                }
            }
        }

        UpdateColors();
    }



    /// <summary>
    /// Updates the color of the current and most recent nearestGrabbable
    /// </summary>
    private void UpdateColors()
    {
        if(nearestGrabbable != lastGrabbable) {
            try {
                lastGrabbable.GetComponent<Outline>().enabled = false;
            }
            catch (NullReferenceException) {
                if (lastGrabbable != null)
                {
                    Debug.LogWarning("Last grabbable " + lastGrabbable.name + " does not have outline component.");
                }
            }

            try {
                nearestGrabbable.GetComponent<Outline>().enabled = true;
            }
            catch (NullReferenceException) {
                if (nearestGrabbable != null)
                {
                    Debug.LogWarning("Grabbable " + nearestGrabbable.name + " does not have outline component.");
                }
            }
            
            lastGrabbable = nearestGrabbable;
        }
    }
}
