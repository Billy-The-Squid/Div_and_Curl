using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ForcePull : MonoBehaviour
{
    XRGrabInteractable[] grabbables;
    XRGrabInteractable grabbed;

    bool isGrabbing = false;
    bool isPulling = false;

    static int grabLayer = 9;
    static int terrainLayer = 8;
    int layerMask;

    //const static LayerMask

    private void Start()
    {
        grabbables = FindObjectsOfType<XRGrabInteractable>();
        grabbed = null;
        layerMask = (1 << grabLayer) | (1 << terrainLayer);

        isPulling = false;
        isGrabbing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPulling)
        {
            SearchForGrabbables();
        }

        
    }

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
}
