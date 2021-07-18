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


    private static string nameToDisplay = "Field Value";
    private static string description = "A thing. What does it do?";
    private static int index;

    public override string displayName { get { return nameToDisplay; } set => throw new System.NotImplementedException("I'm not allowing name changing right now."); }
    public override string displayDescription { get { return description; } set => throw new System.NotImplementedException("I'm not allowing description changing right now"); }
    public override int menuIndex { get { return index; } set { index = value; } }





    // Start is called before the first frame update
    void Start()
    {
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
