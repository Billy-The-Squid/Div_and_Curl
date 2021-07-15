using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldMenuUI : MenuUI
{
    public FieldManager fieldManager;

    // Start is called before the first frame update
    void Start()
    {
        if(fieldManager == null)
        {
            fieldManager = GetComponent<FieldManager>();
            if(fieldManager == null)
            {
                Debug.LogError("FieldMenuUI requires a reference to a FieldManager");
            }
        }

        UIAppearEvent.Invoke(canvas);
        //Debug.Log(this.name + "is invoking UIAppearEvent with " + UIAppearEvent.GetPersistentEventCount() + " listeners.");
    }

    // Update is called once per frame
    void Update()
    {
        ReactToPlayer();
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

    public void NextField()
    {
        // Add support for limited fields. 
        VectorField.FieldType nextField = (VectorField.FieldType)((((int) fieldManager.currentFieldType) + 1) 
            % System.Enum.GetNames(typeof(VectorField.FieldType)).Length);
        fieldManager.SetFieldType(nextField);
    }

    public void PreviousField()
    {
        // Add support for limited fields. 
        VectorField.FieldType prevField = (VectorField.FieldType)((((int)fieldManager.currentFieldType) + 
            System.Enum.GetNames(typeof(VectorField.FieldType)).Length - 1)
            % System.Enum.GetNames(typeof(VectorField.FieldType)).Length);
        fieldManager.SetFieldType(prevField);
    }
}
