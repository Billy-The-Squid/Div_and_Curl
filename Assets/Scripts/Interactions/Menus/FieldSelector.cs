using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FieldSelector : Selector<FieldData>
{
    /// <summary>
    /// The object to be facing.
    /// </summary>
    public Transform playerEyes;
    /// <summary> 
    /// The canvas with all the relevant information.
    /// </summary>
    public Canvas canvas;
    /// <summary>
    /// The background for the menu.
    /// </summary>
    public Collider background;
    /// <summary>
    /// The parent to the background and the canvas. Moves with them. 
    /// </summary>
    public Transform menuObject;
    /// <summary>
    /// The pivot point at the center of the field.
    /// </summary>
    public Transform menuPivot;
    /// <summary>
    /// The distance from the player at which the UI disappears
    /// </summary>
    [Min(1f)]
    public float visibleDistance;
    public float orbitalRadius = 3;
    public float personalSpaceRadius = 0.75f;
    public float adjustmentTorque = 0.01f;
    protected float currentAngularSpeed = 0;
    public float deadzoneWidth = 0.5f;

    public UIEvent UIAppearEvent = new UIEvent();
    public UIEvent UIDisppearEvent = new UIEvent();

    public VectorField field;

    public TextMeshProUGUI nameDisplay, descriptionDisplay;

    protected bool _canSeeCanvas;
    public bool canSeeCanvas
    {
        get => _canSeeCanvas;
        set
        {
            if(_canSeeCanvas != value)
            {
                canvas.enabled = value;
                _canSeeCanvas = value;
                if(background != null)
                {
                    background.enabled = value;
                }
                if(value)
                {
                    UIAppearEvent.Invoke(canvas);
                }
                else
                {
                    UIDisppearEvent.Invoke(canvas);
                }
            }
        }
    }

    public RectTransform nextButton;
    public RectTransform previousButton;

    protected bool isAwake;





    protected override void Start() {
        if (field == null) {
            field = GetComponent<VectorField>();
            if (field == null) {
                Debug.LogError("FieldMenuUI requires a reference to a VectorField");
            }
        }
        canSeeCanvas = true;
        canSeeCanvas = false;

        base.Start();

        UIAppearEvent.Invoke(canvas);
    }



    private void Update()
    {
        if(isAwake)
        {
            ReactToPlayer();
        }
    }



    protected override void ChangeSelection()
    {
        if (current >= available.Length) {
            Debug.LogError("Empty available array detected.");
            return;
        }
        if(available[current] == null)
        {
            Debug.LogWarning("Empty array entry detected. This may or may not have been intentional.");
            field.fieldType = VectorField.FieldType.Empty;
            canSeeCanvas = false;
            isAwake = false; // This variable persists until a new scene load.
            return;
        }

        field.fieldType = available[current].field;

        nameDisplay.SetText(available[current].name);
        descriptionDisplay.SetText(available[current].description);
    }



    public void LoadScene(FieldScene scene)
    {
        isAwake = true;
        available = scene.fieldArray;
        current = 0;
    }



    protected override void ChangeAvailable()
    {
        base.ChangeAvailable();

        nextButton.gameObject.SetActive(HasNext());
        previousButton.gameObject.SetActive(HasPrevious());
    }



    protected void ReactToPlayer()
    {
        // Check relative positions
        Vector3 pivotDist = planeDist(menuPivot.transform.position, playerEyes.position);
        Vector3 menuDist = planeDist(menuObject.transform.position, playerEyes.position);
        float angleBetween = Vector3.SignedAngle(planeDist(menuPivot.transform.position, menuObject.transform.position), pivotDist, menuPivot.transform.up);
        // If we're nowhere close, rotate to face the player.
        if (menuDist.magnitude > personalSpaceRadius)
        {
            currentAngularSpeed = Mathf.Abs(angleBetween) * adjustmentTorque;
            
        }
        // If we're definitely in the bubble, move away. 
        else if (Mathf.Abs(menuDist.magnitude - personalSpaceRadius) > deadzoneWidth)
        {
            currentAngularSpeed = -Mathf.Abs(angleBetween) * adjustmentTorque;
            //Debug.Log("Getting close");
        }
        // If we're in the middle, do nothing. 
        else
        {
            currentAngularSpeed = 0;
        }
        menuPivot.transform.forward = Vector3.RotateTowards(menuPivot.transform.forward, pivotDist.normalized, currentAngularSpeed * Time.deltaTime, 0);
        menuObject.transform.forward = menuDist.normalized;

        // Needs to be able to turn to face the player when inside the field as well. Maybe add support for physics movement too?

        
        
        // Update whether it's visible.

        if(available[current] == null) {
            canSeeCanvas = false;
        }
        // If the field is empty, it shouldn't ever disappear.
        else if (available[current].field == VectorField.FieldType.Empty) 
        {
            canSeeCanvas = true;
        }
        // Otherwise, it should.
        else
        {
            // Close the display if the player is far away. 
            if (menuDist.magnitude <= visibleDistance || pivotDist.magnitude <= orbitalRadius)
            {
                canSeeCanvas = true;
                { // Delete me if I don't seem necessary
                    //if (!canvas.enabled)
                    //{
                    //    canvas.enabled = true;
                    //    UIAppearEvent.Invoke(canvas);
                    //    if (background != null)
                    //    {
                    //        background.enabled = true;
                    //    }
                    //}
                }
            }
            else
            {
                if (canvas.enabled)
                {
                    canSeeCanvas = false;
                    { // Delete me if I don't seem necessary
                      //canvas.enabled = false;
                      //UIDisppearEvent.Invoke(canvas);
                      //if (background != null)
                      //{
                      //    background.enabled = false;
                      //}
                    }
                }
            }
        }

        Vector3 planeDist(Vector3 vect1, Vector3 vect2)
        {
            Vector3 dist = vect1 - vect2;
            return new Vector3(dist.x, 0, dist.z);
        }
    }



    //public void Sleep()
    //{
    //    isAwake = false;
    //    field.fieldType = VectorField.FieldType.Empty;
    //    canSeeCanvas = false;
    //}

    //public void Wake()
    //{
    //    ChangeSelection();
    //    isAwake = true;
    //}
}
