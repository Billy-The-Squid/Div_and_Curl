using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VectorField), typeof(FieldZone))]
public class Pointer : FieldDetector
{
    [Header("Detector")]
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
    protected override void Start()
    {
        // Find the local field.
        if(localField == null) {
            localField = GetComponent<VectorField>();
        }

        localField.preCalculations += PreCalculate;

        localField.enabled = inField;

        detectorReadout = new VectorReadout("Field value");

        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

        if(!inField) { return; }

        localField.vectorsBuffer.GetData(vecArray);
        ((VectorReadout)detectorReadout).output = vecArray[0];
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
        ((VectorReadout)detectorReadout).isActive = true;
        base.EnteredField(graph);
    }

    public override void ExitedField(VectorField graph)
    {
        localField.enabled = false;
        ((VectorReadout)detectorReadout).isActive = false;
        base.ExitedField(graph);
    }
}

public class VectorReadout : DetectorReadout
{
    public string name;
    public Vector3 output;
    public bool isActive;

    public VectorReadout(string name)
    {
        this.name = name;
        this.output = new Vector3(0,0,0);
        isActive = false;
    }

    public override string GetName()
    {
        return name;
    }

    public override string GetReadout()
    {
        if(isActive)
        {
            return string.Format("({0:1},{1:1},{2:1})", output.x, output.z, output.y);
        }
        else
        {
            return "INACTIVE";
        }
    }
}
