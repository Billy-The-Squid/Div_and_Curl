using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldMenuUI : MenuUI
{
    public VectorField field;

    public List<VectorField.FieldType> fieldsAvailable;

    // Start is called before the first frame update
    void Start()
    {
        if(field == null) {
            field = GetComponent<VectorField>();
            if(field == null) {
                Debug.LogError("FieldMenuUI requires a reference to a VectorField");
            }
        }

        UIAppearEvent.Invoke(canvas);
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

    /// <summary>
    /// Retrieves the value of the next field.
    /// </summary>
    /// <returns>Either the enum index of the next field or -1 if there is no next field. </returns>
    protected int FindNextField()
    {
        int cursor = fieldsAvailable.IndexOf(field.fieldType);

        if(cursor + 1 < fieldsAvailable.Count) {
            return (int)fieldsAvailable[cursor + 1];
        } 
        else {
            return -1;
        }
    }

    public void BringUpNextField() {
        int nextField;

        nextField = FindNextField();

        if(nextField != -1) {
            field.fieldType = (VectorField.FieldType)nextField;
        }
    }

    protected int FindPrevField()
    {
        int cursor = fieldsAvailable.IndexOf(field.fieldType);

        if (cursor - 1 >= 0)
        {
            return (int)fieldsAvailable[cursor - 1];
        }
        else
        {
            return -1;
        }
    }

    public void BringUpPreviousField()
    {
        int prevField;

        prevField = FindPrevField();

        if(prevField != -1)
        {
            field.fieldType = (VectorField.FieldType)prevField;
        }
    }

    public void LoadScene(FieldScene scene)
    {
        fieldsAvailable = scene.fieldList;
        if(fieldsAvailable != null && fieldsAvailable.Count > 0)
        {
            field.fieldType = fieldsAvailable[0];
        }
    }
}
