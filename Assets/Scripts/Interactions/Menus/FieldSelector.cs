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
    /// The distance from the player at which the UI disappears
    /// </summary>
    [Min(1f)]
    public float visibleDistance;

    public UIEvent UIAppearEvent = new UIEvent();
    public UIEvent UIDisppearEvent = new UIEvent();

    public VectorField field;

    public TextMeshProUGUI nameDisplay, descriptionDisplay;





    protected override void Start()
    {
        if (field == null)
        {
            field = GetComponent<VectorField>();
            if (field == null)
            {
                Debug.LogError("FieldMenuUI requires a reference to a VectorField");
            }
        }

        base.Start();

        UIAppearEvent.Invoke(canvas);
    }



    private void Update()
    {
        ReactToPlayer();
    }



    protected override void ChangeSelection()
    {
        if (current >= available.Length || available[current] == null) {
            Debug.LogError("Empty available array or array entry detected.");
            return;
        }

        field.fieldType = available[current].field;

        nameDisplay.SetText(available[current].name);
        descriptionDisplay.SetText(available[current].description);
    }



    public void LoadScene(FieldScene scene)
    {
        available = scene.fieldArray;
        current = 0;
    }



    protected void ReactToPlayer()
    {
        // Rotate to face the player
        Vector3 displacement = transform.position - playerEyes.position;
        Vector3 planeDistance = new Vector3(displacement.x, 0, displacement.z);
        transform.forward = planeDistance.normalized;

        // Needs to be able to turn to face the player when inside the field as well. Maybe add support for physics movement too?

        // Close the display if the player is far away. 
        if (planeDistance.magnitude <= visibleDistance)
        {
            if (!canvas.enabled)
            {
                canvas.enabled = true;
                UIAppearEvent.Invoke(canvas);
                if (background != null)
                {
                    background.enabled = true;
                }
            }
        }
        else
        {
            if (canvas.enabled)
            {
                canvas.enabled = false;
                UIDisppearEvent.Invoke(canvas);
                if (background != null)
                {
                    background.enabled = false;
                }
            }
        }
    }
}
