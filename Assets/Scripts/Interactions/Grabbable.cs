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
    } // Remove references to this, use currentHighlight instead. 

    protected Dictionary<GameObject, Highlight> highlighters;

    public Color normalHighlight = new Color(0, 1, 1, 1); // cyan
    public Color invalidHighlight = new Color(1, 0, 0, 1); // red
    public enum Highlight { Normal, None, Invalid };
    protected Highlight _currentHighlight = Highlight.Normal;
    public Highlight currentHighlight
    {
        get => _currentHighlight;
        set
        {
            if (_currentHighlight != value)
            {
                switch(value)
                {
                    case Highlight.Normal:
                        outline.OutlineColor = normalHighlight;
                        _currentHighlight = Highlight.Normal;
                        isOutlined = true;
                        break;
                    case Highlight.Invalid:
                        outline.OutlineColor = invalidHighlight;
                        _currentHighlight = Highlight.Invalid;
                        isOutlined = true;
                        break;
                    case Highlight.None:
                        isOutlined = false;
                        _currentHighlight = Highlight.None;
                        break;
                    default:
                        break;
                }
            }
        }
    }






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

        highlighters = new Dictionary<GameObject, Highlight>();
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        // Store the positions (is this necessary?)
        nativeAttachRotation = attachTransform.localRotation;
        nativeAttachPosition = attachTransform.localPosition;
        // Move the attach transform
        attachTransform.rotation = args.interactor.attachTransform.rotation;
        attachTransform.position = args.interactor.attachTransform.position;

        //// Make sure is outlined
        //if(currentHighlight != Highlight.None)
        //{
        //    currentHighlight = Highlight.None;
        //}

        CheckHighlight();

        base.OnSelectEntering(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        // Reset positions (is this necessary?)
        attachTransform.localRotation = nativeAttachRotation;
        attachTransform.localPosition = nativeAttachPosition;
    }

    public void AddHighlighter(GameObject highlighter, Highlight highlight)
    {
        //Debug.Log(this.GetInstanceID() + " AddHighlighter called");
        if(!highlighters.ContainsKey(highlighter))
        {
            //Debug.Log("Changing highlight to normal");
            //Debug.Log(this.GetInstanceID() + " Adding highlighter");
            highlighters.Add(highlighter, highlight);
            //currentHighlight = Highlight.Normal;
            CheckHighlight();
        }
        else if(highlighters[highlighter] != highlight)
        {
            //Debug.LogWarning("Logging changed highlight: " + highlight);
            highlighters[highlighter] = highlight;
            CheckHighlight();
        }
    }

    public void RemoveHighlighter(GameObject highlighter)
    {
        if(highlighters.ContainsKey(highlighter))
        {
            highlighters.Remove(highlighter);
            //if(highlighters.Count == 0)
            //{
            //    currentHighlight = Highlight.None;
            //    //Debug.Log("Removing current highlight");
            //}
            CheckHighlight();
        }
    }


    /// <summary>
    /// Checks the highlight list to see what color the highlight should be.
    /// </summary>
    protected void CheckHighlight()
    {
        if(highlighters.ContainsValue(Highlight.Normal))
        {
            currentHighlight = Highlight.Normal;
        }
        else if(highlighters.ContainsValue(Highlight.Invalid))
        {
            currentHighlight = Highlight.Invalid;
        }
        else
        {
            currentHighlight = Highlight.None;
        }
    }
}
