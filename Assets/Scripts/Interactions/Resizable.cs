using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;


/// <summary>
/// These objects, once selected, can be resized. 
/// 
/// To use this, bind the "Select Entered" and "Select Exited" interaction events of the 
/// <cref>XRBaseInteractable</cref> to SetSelectedTrue and SetSelectedFalse, respectively.
/// </summary>
[RequireComponent(typeof(XRBaseInteractable))]
public class Resizable : MonoBehaviour
{
    /// <summary>
    /// The minimum radius of the object
    /// </summary>
    [SerializeField]
    private float minRad = 0.05f;
    /// <summary>
    /// The maximum radius of the object
    /// </summary>
    [SerializeField]
    private float maxRad = 1f;
    /// <summary>
    /// The current radius of the object
    /// </summary>
    [SerializeField] // Find some way to clamp here
    public float radius;

    /// <summary>
    /// The delay (in seconds) before resizing is allowed again.
    /// </summary>
    [SerializeField, Range(0, 10)]
    float waitBeforeResized = 0.2f;

    /// <summary>
    /// The time at which the object was last resized. 
    /// </summary>
    private float lastResized;

    /// <summary>
    /// The increments by which the object is resized.
    /// </summary>
    [SerializeField]
    float resizeIncrement = 0.1f;

    protected Transform attachTransform;

    public struct SizeChange
    {
        public bool isValid;
        public float previousRadius;
        public float newRadius;
    }




    // Is there a way to do some of this in edit mode?
    private void Start()
    {
        lastResized = Time.time;

        radius = Mathf.Clamp(radius, minRad, maxRad);
        transform.localScale = Vector3.one * radius;

        StartCoroutine(FindAttach());
    }

    protected IEnumerator FindAttach()
    {
        yield return null;

        if (GetComponent<Grabbable>() != null) {
            attachTransform = GetComponent<Grabbable>().attachTransform;
        }
        else {
            Debug.LogWarning("Resizable object " + name + " does not have a Grabbable component");
        }

        if(attachTransform == null)
        {
            Debug.LogWarning("Resizable " + name + " couldn't find an attach transform");
        }
        
    }

    /// <summary>
    /// Increases the size of the object
    /// </summary>
    public SizeChange SizeUp() {
        SizeChange change = new SizeChange();
        if (Time.time > lastResized + waitBeforeResized)
        {
            change.isValid = true;
            change.previousRadius = transform.localScale.x;
            lastResized = Time.time;
            radius = Mathf.Clamp(radius + resizeIncrement, minRad, maxRad);
            transform.localScale = Vector3.one * radius;
            change.newRadius = radius;

            attachTransform.localPosition *= change.previousRadius / radius;
        }
        else
        {
            change.isValid = false;
        }
        return change;
    }

    /// <summary>
    /// Decreases the size of the object
    /// </summary>
    public SizeChange SizeDown() {
        SizeChange change = new SizeChange();
        if(Time.time > lastResized + waitBeforeResized)
        {
            change.isValid = true;
            change.previousRadius = transform.localScale.x;
            lastResized = Time.time;
            radius = Mathf.Clamp(radius - resizeIncrement, minRad, maxRad);
            transform.localScale = Vector3.one * radius;
            change.newRadius = radius;

            attachTransform.localPosition *= change.previousRadius / radius;
        }
        else
        {
            change.isValid = false;
        }
        return change;
    }
}
