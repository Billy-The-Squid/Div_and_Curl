using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{   
    // Honestly, this stuff should just be included in VectorField.
    public VectorField currentField;

    [SerializeField]
    public VectorField.FieldType currentFieldType { get; protected set; }
    
    public bool fieldNeedsUpdate;

    protected bool initiallyDynamic;




    private void Start()
    {
        currentField.preSetPositions += UpdateFieldType;
    }

    public void UpdateFieldType()
    {
        if(!fieldNeedsUpdate) { return; }

        initiallyDynamic = currentField.isDynamic;
        currentField.isDynamic = true;
        currentField.preDisplay += RevertDynamic;
        currentField.fieldType = currentFieldType;

        fieldNeedsUpdate = false;
    }

    public void RevertDynamic()
    {
        currentField.isDynamic = initiallyDynamic;
        currentField.preDisplay -= RevertDynamic;
    }

    public void SetFieldType(VectorField.FieldType type)
    {
        currentFieldType = type;
        fieldNeedsUpdate = true;
    }
}
