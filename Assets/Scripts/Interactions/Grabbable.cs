using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Outline))]
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

    protected Outline outline { get; set; }
    public bool isOutlined
    {
        get => outline.enabled;
        set
        {
            //Debug.Log(this.GetInstanceID() + " Setting isOutlined " + value);
            if(!(outline.enabled == value))
            {
                //Debug.Log(this.GetInstanceID() + " confirmed state change");
                if(!value || !isSelected)
                {
                    //Debug.Log(this.GetInstanceID()+ " Enabling outline");
                    outline.enabled = value;
                }
            }
            //Debug.Log(GetInstanceID() + " enabled? " + outline.enabled);
        }
    }

    protected List<GameObject> highlighters;





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

        if(outline == null)
        {
            outline = GetComponent<OutlineExtension>();
        }
        if(outline == null)
        {
            outline = GetComponent<Outline>();
        }

        highlighters = new List<GameObject>();
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        // Store the positions (is this necessary?)
        nativeAttachRotation = attachTransform.localRotation;
        nativeAttachPosition = attachTransform.localPosition;
        // Move the attach transform
        attachTransform.rotation = args.interactor.attachTransform.rotation;
        attachTransform.position = args.interactor.attachTransform.position;

        // Make sure not outlined
        if(isOutlined)
        {
            isOutlined = false;
        }

        base.OnSelectEntering(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        // Reset positions (is this necessary?)
        attachTransform.localRotation = nativeAttachRotation;
        attachTransform.localPosition = nativeAttachPosition;

        //// Update outline following HandManager
        //if(hoveringInteractors.Count != 0) { isOutlined = true; }
    }

    public void AddHighlighter(GameObject highlighter)
    {
        //Debug.Log(this.GetInstanceID() + " AddHighlighter called");
        if(!highlighters.Contains(highlighter))
        {
            //Debug.Log(this.GetInstanceID() + " Adding highlighter");
            highlighters.Add(highlighter);
            isOutlined = true;
        }
    }

    public void RemoveHighlighter(GameObject highlighter)
    {
        if(highlighters.Contains(highlighter))
        {
            highlighters.Remove(highlighter);
            if(highlighters.Count == 0)
            {
                isOutlined = false;
            }
        }
    }
}
