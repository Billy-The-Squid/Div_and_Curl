using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FieldSelector : Selector<FieldData>
{
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



    protected override void ReactToPlayer()
    {
        // Rotate to face the player
        Vector3 displacement = transform.position - playerEyes.position;
        Vector3 planeDistance = new Vector3(displacement.x, 0, displacement.z);
        transform.forward = planeDistance.normalized;

        // Needs to be able to turn to face the player when inside the field as well. Maybe add support for physics movement too?

        base.ReactToPlayer();
    }
}
