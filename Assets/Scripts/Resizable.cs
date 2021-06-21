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
    /// Is the object currently selected? Enables resizing. 
    /// </summary>
    private bool isSelected;

    /// <summary>
    /// The input action asset with "Resize Object" defined. 
    /// </summary>
    [SerializeField]
    InputActionAsset inputActions;
    // Better to ask for the action map itself. 

    /// <summary>
    /// The action that triggers resizing. Must be a 2D axis. 
    /// </summary>
    private InputAction resizeAction;

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




    // Is there a way to do some of this in edit mode?
    private void Start()
    {
        lastResized = Time.time;

        radius = Mathf.Clamp(radius, minRad, maxRad);
        transform.localScale = Vector3.one * radius;
        isSelected = false;

        resizeAction = inputActions.FindActionMap("XRI RightHand").FindAction("Resize Object");
    }

    private void Update()
    {
        //Debug.Log("Phase: " + resizeAction.phase);
        //Debug.Log("Resize action value: " + resizeAction.ReadValue<Vector2>());
        if (isSelected && (Time.time > lastResized + waitBeforeResized) && (resizeAction.phase == InputActionPhase.Started || resizeAction.phase == InputActionPhase.Performed))
        {
            float current = resizeAction.ReadValue<Vector2>().y;
            if (current > 0f)
            {
                SizeUp();
                lastResized = Time.time;
            } else if(current < 0f)
            {
                SizeDown();
                lastResized = Time.time;
            }
        }
    }



    /// <summary>
    /// Informs the resizable object that it is currently selected. 
    /// </summary>
    public void SetSelectedTrue()
    {
        isSelected = true;
    }
    
    /// <summary>
    /// Informs the resizable object that it is no longer currently selected. 
    /// </summary>
    public void SetSelectedFalse () {
        isSelected = false;
    }

    /// <summary>
    /// Increases the size of the object
    /// </summary>
    public void SizeUp() {
        radius = Mathf.Clamp(radius + resizeIncrement, minRad, maxRad);
        transform.localScale = Vector3.one * radius;
    }

    /// <summary>
    /// Decreases the size of the object
    /// </summary>
    public void SizeDown() {
        radius = Mathf.Clamp(radius - resizeIncrement, minRad, maxRad);
        transform.localScale = Vector3.one * radius;
    }
}
