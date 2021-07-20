using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(OutlineExtension))]
public abstract class Grabbable : XRGrabInteractable
{
    /// <summary>
    /// The name displayed in the selector menu
    /// </summary>
    public abstract string displayName { get; set; } // Should be static? Or something?

    /// <summary>
    /// The description to be displayed in the selector menu.
    /// </summary>
    public abstract string displayDescription { get; set; }

    /// <summary>
    /// The index used to identify the grabbable by the detector menu.
    /// </summary>
    public abstract int menuIndex { get; set; }

    protected Quaternion nativeAttachRotation;
    protected Vector3 nativeAttachPosition;





    protected virtual void Start()
    {
        if (attachTransform == null || attachTransform == transform)
        {
            attachTransform = (new GameObject()).GetComponent<Transform>();
            attachTransform.parent = transform;
            attachTransform.localPosition = new Vector3(0, 0, 0);
            attachTransform.localRotation = Quaternion.Euler(0,0,0);
            attachTransform.name = "Generated attach point";
        }
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        nativeAttachRotation = attachTransform.localRotation;
        nativeAttachPosition = attachTransform.localPosition;
        attachTransform.rotation = args.interactor.attachTransform.rotation;
        attachTransform.position = args.interactor.attachTransform.position;

        base.OnSelectEntering(args);
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);

        attachTransform.localRotation = nativeAttachRotation;
        attachTransform.localPosition = nativeAttachPosition;
    }
}
