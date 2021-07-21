using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

//[RequireComponent(typeof(XRSocketInteractor))]
public class TrashCan : MonoBehaviour
{
    // The mesh to disappear when we're hovering an object.
    [SerializeField]
    public GameObject lid;

    // The collider inside which objects can be dropped.
    [SerializeField]
    protected Collider hoverZone;

    // What's hovering right now?
    protected List<XRGrabInteractable> hovering = new List<XRGrabInteractable>();



    private void Update()
    {
        if(hoverZone == null) {
            Debug.LogWarning("Trash can requires a collider set to trigger");
        }

        // Look for things that have been dropped.
        List<XRGrabInteractable> dispose = new List<XRGrabInteractable>();
        foreach(XRGrabInteractable hovered in hovering) {
            if(!hovered.isSelected) {
                dispose.Add(hovered);
                hovered.enabled = false;
                StartCoroutine(DestroyInteractable(hovered.gameObject));
            }
        }
        // Remove the objects from the list.
        foreach(XRGrabInteractable hovered in dispose) {
            if(hovering.Contains(hovered)) {
                hovering.Remove(hovered);
            }
        }

        // Display or don't display the lid.
        if (hovering.Count == 0) {
            lid.SetActive(true);
        }
        else {
            lid.SetActive(false);
        }
    }

    // When something enters the zone, add it to the list to monitor. 
    private void OnTriggerEnter(Collider other) {
        if(other.GetComponent<XRGrabInteractable>() != null) {
            XRGrabInteractable grabbable = other.GetComponent<XRGrabInteractable>();

            if(grabbable != null && grabbable.isSelected && grabbable.selectingInteractor is XRDirectInteractor) {
                hovering.Add(grabbable);
            }
        }
    }

    // When something leaves the zone, stop monitoring it.
    private void OnTriggerExit(Collider other) {
        XRGrabInteractable grabbable = other.GetComponent<XRGrabInteractable>();
        if (grabbable != null && hovering.Contains(grabbable)) {
            hovering.Remove(grabbable);
        }
    }



    /// <summary>
    /// A patient method to dispose of objects.
    /// </summary>
    /// <param name="obj">The object to be destroyed.</param>
    /// <returns></returns>
    public static IEnumerator DestroyInteractable(GameObject obj) {
        // Make the object disappear
        obj.SetActive(false);

        // Let the Interaction system forget it exists
        yield return new WaitForSeconds(1f);

        // Destroy it.
        Destroy(obj);
    }
}
