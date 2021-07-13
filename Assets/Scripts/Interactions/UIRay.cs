using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class UIRay : MonoBehaviour
{
    /// <summary>
    /// Which interactable UIs are present in the scene?
    /// </summary>
    public MenuUI[] UIs;

    /// <summary>
    /// Which UIs are Visible?
    /// </summary>
    protected List<Canvas> UIsVisible; // This should really be a set, not a list. 

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

        UIsVisible = new List<Canvas>();

        foreach(MenuUI menu in UIs)
        {
            menu.UIAppear += SetUIVisible;
            menu.UIDisappear += RemoveUIVisible;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRay();
    }



    /// <summary>
    /// Renders or does not render the UI depending on the value of <cref>UIVisible</cref>.
    /// </summary>
    protected void UpdateRay()
    {
        ray.enabled = !(UIsVisible.Count == 0);
    }



    /// <summary>
    /// Adds a visible UI to the user's list.
    /// </summary>
    public bool SetUIVisible(Canvas UI) {
        if(!UIsVisible.Contains(UI)) {
            UIsVisible.Add(UI);
        }
        return true;
    }

    /// <summary>
    /// Removes a UI from the user's list of visible UIs. Returns false if the object is not
    /// in the list.
    /// </summary>
    public bool RemoveUIVisible(Canvas UI) {
        if(UIsVisible.Contains(UI)) {
            UIsVisible.Remove(UI);
            return true;
        } else { return false; }
    }
}
