using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

//[RequireComponent(typeof(XRSocketInteractor))]
public class TrashCan : MonoBehaviour
{
    //[SerializeField]
    //public XRSocketInteractor socket;

    [SerializeField]
    public GameObject lid;

    [SerializeField]
    protected Collider hoverZone;

    protected List<XRGrabInteractable> hovering = new List<XRGrabInteractable>();



    private void Update()
    {
        foreach(XRGrabInteractable hovered in hovering)
        {
            if(!hovered.isSelected)
            {
                hovering.Remove(hovered);
                hovered.enabled = false;
                StartCoroutine(DestroyInteractable(hovered.gameObject));
            }
        }

        if (hovering.Count == 0)
        {
            lid.SetActive(true);
        }
        else
        {
            lid.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<XRGrabInteractable>() != null)
        {
            XRGrabInteractable grabbable = other.GetComponent<XRGrabInteractable>();

            if(grabbable != null && grabbable.isSelected && grabbable.selectingInteractor is XRDirectInteractor)
            {
                hovering.Add(grabbable);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        XRGrabInteractable grabbable = other.GetComponent<XRGrabInteractable>();
        if (grabbable != null && hovering.Contains(grabbable))
        {
            hovering.Remove(grabbable);
        }
    }



    protected IEnumerator DestroyInteractable(GameObject obj)
    {
        yield return new WaitForSeconds(1f);

        obj.SetActive(false);

        //yield return null; // Wait for next frame?

        //Destroy(obj);
    }
}
