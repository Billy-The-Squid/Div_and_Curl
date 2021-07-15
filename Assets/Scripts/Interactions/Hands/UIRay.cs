using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class UIRay : MonoBehaviour
{
    /// <summary>
    /// The UI Ray Interactor.
    /// </summary>
    public XRRayInteractor ray;






    // Start is called before the first frame update
    void Start()
    {
        if (ray == null) {
            ray = GetComponent<XRRayInteractor>();
        }
    }

    /// <summary>
    /// Enable the ray and raycasting.
    /// </summary>
    public void EnableRay() {
        ray.enabled = true;
        //Debug.Log("Enabling ray");
    }

    /// <summary>
    /// Render the raycast.
    /// </summary>
    public void DrawRay() {
        ray.GetComponent<XRInteractorLineVisual>().enabled = true;
        //Debug.Log("Drawing ray");
    }

    /// <summary>
    /// Disable the ray and raycasting.
    /// </summary>
    public void DisableRay() {
        ray.enabled = false;
        //Debug.Log("Disabling ray");
    } 

    /// <summary>
    /// Stop rendering the raycast.
    /// </summary>
    public void StopDrawRay() {
        ray.GetComponent<XRInteractorLineVisual>().enabled = false;
        //Debug.Log("Stopped drawing ray");
    }
}
