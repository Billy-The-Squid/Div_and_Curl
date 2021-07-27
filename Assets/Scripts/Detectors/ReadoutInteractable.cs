using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ReadoutInteractable : XRGrabInteractable
{
    protected Vector3 nativeAttachPosition;
    protected Quaternion nativeAttachRotation;

    private void Start()
    {
        if (attachTransform == null || attachTransform == transform)
        {
            attachTransform = (new GameObject()).GetComponent<Transform>();
            attachTransform.parent = transform;
            attachTransform.localPosition = new Vector3(0, 0, 0);
            attachTransform.localRotation = Quaternion.Euler(0, 0, 0);
            attachTransform.name = "Generated attach point";
        }
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        // Store the positions (is this necessary?)
        nativeAttachRotation = attachTransform.localRotation;
        nativeAttachPosition = attachTransform.localPosition;
        // Move the attach transform
        attachTransform.rotation = args.interactor.attachTransform.rotation;
        attachTransform.position = args.interactor.attachTransform.position;

        base.OnSelectEntering(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        // Reset positions (is this necessary?)
        attachTransform.localRotation = nativeAttachRotation;
        attachTransform.localPosition = nativeAttachPosition;
    }
}
