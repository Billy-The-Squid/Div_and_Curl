using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


// Needs documentation
public class ForcePull : MonoBehaviour
{
    // Every object in the scene that can be grabbed
    XRGrabInteractable[] grabbables;
    // The one that will be pulled if you pull it. 
    XRGrabInteractable nearestGrabbable;

    // Should be false if currently pulling or holding something.
    bool handBusy = false;

    static readonly int grabLayer = 9;
    static readonly int terrainLayer = 8;
    int layerMask;

    [SerializeField]
    InputActionAsset actionAsset;
    protected InputAction grab, rightGrab;

    protected enum Hands { Right, Left }
    [SerializeField]
    Hands hand;
    protected string[] handNames = { "Right", "Left" };

    [SerializeField]
    //float pullAcceleration;
    float pullSpeed;


    private void Start()
    {
        // Initialize the set of possible grab
        // This should be done frame-by-frame if objects are being added. 
        grabbables = FindObjectsOfType<XRGrabInteractable>();
        nearestGrabbable = null;
        layerMask = (1 << grabLayer) | (1 << terrainLayer);

        handBusy = false;
        string handName = handNames[(int)hand];
        grab = actionAsset.FindActionMap("XRI " + handName + "Hand").FindAction("Select");
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePull();

        // Search if your hands aren't occupied. 
        if (!handBusy)
        {
            nearestGrabbable = SearchForGrabbables();
        }

        // nearestGrabbable.changeColor

        // This should use a separate trigger, probably.
        if(handBusy)
        {
            if(!nearestGrabbable.isSelected)
            {
                //nearestGrabbable.GetComponent<Rigidbody>().MovePosition(transform.position);
                nearestGrabbable.GetComponent<Rigidbody>().MovePosition(Vector3.MoveTowards(nearestGrabbable.transform.position, transform.position, pullSpeed * Time.deltaTime));
                //nearestGrabbable.GetComponent<Rigidbody>().velocity += 
                //    Time.deltaTime * pullAcceleration * (transform.position - nearestGrabbable.transform.position).normalized;
            }
        }
    }

    /// <summary>
    /// Sets the value of handBusy based 
    /// </summary>
    private void UpdatePull()
    { // This could also be done with interactiion events
        handBusy = !(grab.phase == InputActionPhase.Waiting);
    }

    // Searches for a grabbable within 
    private XRGrabInteractable SearchForGrabbables()
    {
        Transform nearest = null;
        float dist = float.MaxValue;
        
        foreach(XRGrabInteractable obj in grabbables)
        {
            // Is it in the general direction?
            if(Vector3.Angle((obj.transform.position - transform.position), transform.forward) <= 30) // degrees
            {
                // Do we have line-of-sight?
                Physics.Raycast(transform.position, obj.transform.position - transform.position, out RaycastHit hit, 40f, layerMask);
                if (hit.transform.Equals(obj.transform))
                {
                    // Is it the closest?
                    if (nearest == null)
                    {
                        nearest = hit.transform;
                        dist = hit.distance;
                    }
                    else
                    {
                        if (hit.distance < dist)
                        {
                            nearest = hit.transform;
                            dist = hit.distance;
                        }
                    }
                }
            }
        }
        return nearest.GetComponent<XRGrabInteractable>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        
    }
}
