using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VectorField), typeof(FieldZone))]
public class Pointer : FieldDetector
{
    /// <summary>
    /// The field used to generate the detector's display.
    /// </summary>
    public VectorField localField;

    /// <summary>
    /// Array storing the local vector. 
    /// </summary>
    protected Vector3[] vecArray = new Vector3[1];





    // Start is called before the first frame update
    void Start()
    {
        displayName = "Field value";

        // Find the local field.
        if(localField == null) {
            localField = GetComponent<VectorField>();
        }

        localField.preCalculations += PreCalculate;

        localField.enabled = inField;

        quantityName = "Magnitude";
    }

    // Update is called once per frame
    void Update()
    {

        if(!inField) { return; }

        localField.vectorsBuffer.GetData(vecArray);
        detectorOutput = vecArray[0].magnitude;
    }

    /// <summary>
    /// The functions that need to be called before the vector field does its computations. 
    /// </summary>
    void PreCalculate()
    {
        localField.zone.fieldOrigin = detectedField.zone.fieldOrigin;
        localField.fieldType = detectedField.fieldType; 
    }

    public override void EnteredField(VectorField graph)
    {
        localField.enabled = true;
        base.EnteredField(graph);
    }

    public override void ExitedField(VectorField graph)
    {
        localField.enabled = false;
        detectorOutput = 0f;
        base.ExitedField(graph);
    }
}
