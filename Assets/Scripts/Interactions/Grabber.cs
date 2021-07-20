using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Grabber : XRDirectInteractor
{
    [System.NonSerialized]
    public Quaternion nativeAttachRotation;

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        nativeAttachRotation = attachTransform.rotation;
        attachTransform.rotation = args.interactable.transform.rotation;

        base.OnSelectEntering(args);
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        attachTransform.rotation = nativeAttachRotation;

        base.OnSelectExiting(args);
    }
}
