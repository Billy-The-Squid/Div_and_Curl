using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OutlineExtension : Outline
{
    //private void Reset()
    //{
    //    Grabbable grabComponent = GetComponent<Grabbable>();
    //    // Guarantees the existence of a GrabInteractable.
    //    if(grabComponent != null)
    //    {
    //        GetComponent<XRGrabInteractable>().hoverEntered.AddListener(TurnOutlineOn);
    //        GetComponent<XRGrabInteractable>().hoverExited += TurnOutlineOff;
    //    }
    //}

    public void TurnOutlineOn()
    {
        this.enabled = true;
    }

    public void TurnOutlineOff()
    {
        this.enabled = false;
    }
}
