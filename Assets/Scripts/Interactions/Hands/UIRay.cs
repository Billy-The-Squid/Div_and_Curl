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

    // Update is called once per frame
    void Update()
    {
        //UpdateRay();
    }



    ///// <summary>
    ///// Renders or does not render the UI depending on the value of <cref>UIVisible</cref>.
    ///// </summary>
    //protected void UpdateRay()
    //{
    //    ray.enabled = !(UIsVisible.Count == 0);
    //}



    ///// <summary>
    ///// Adds a visible UI to the user's list.
    ///// </summary>
    //public void SetUIVisible(Canvas UI) {
    //    if(!UIsVisible.Contains(UI)) {
    //        UIsVisible.Add(UI);
    //    }
    //    //return true;
    //    //Debug.Log("Adding canvas: " + UI.name);
    //    Debug.LogWarning("Please remove the UIRay.SetUIVisible and .RemoveUIVisible methods and subscribe HandManager methods instead");
    //}

    ///// <summary>
    ///// Removes a UI from the user's list of visible UIs. Returns false if the object is not
    ///// in the list.
    ///// </summary>
    //public void RemoveUIVisible(Canvas UI) {
    //    if(UIsVisible.Contains(UI)) {
    //        UIsVisible.Remove(UI);
    //        //return true;
    //    }
    //    //else { return false; }
    //    //Debug.Log("Remove canvas: " + UI.name);
    //}
}
