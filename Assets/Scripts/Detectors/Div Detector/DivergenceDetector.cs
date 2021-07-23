using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(VectorField), typeof(DerivativeZone), typeof(DivRender))]
public class DivergenceDetector : FieldDetector
{
    [Header("Detector")]
    /// <summary>
    /// The field used to do the computations
    /// </summary>
    [SerializeField]
    VectorField computeField;

    /// <summary>
    /// The calculated divergence.
    /// </summary>
    public float divergence { get; protected set; }

    /// <summary>
    /// The compute shader used to compute the divergence.
    /// </summary>
    [SerializeField]
    ComputeShader divergenceComputer;

    /// <summary>
    /// The compute buffer used to store the divergence components.
    /// 
    /// This contains a single Vector3, with the x component storing
    /// dVx/dx, etc. Add the components to get the divergence. 
    /// </summary>
    protected ComputeBuffer divBuffer;
    private Vector3[] tempDivArray = new Vector3[1];

    /// <summary>
    /// The component used to make the visual display of the flux. 
    /// </summary>
    [SerializeField]
    protected DivRender divRenderer;

    private static string nameToDisplay = "Divergence";
    private static string description = "A thing. What does it do?";
    private static int index;

    public override string displayName { get { return nameToDisplay; } set => throw new System.NotImplementedException("I'm not allowing name changing right now."); }
    public override string displayDescription { get { return description; } set => throw new System.NotImplementedException("I'm not allowing description changing right now"); }
    public override int menuIndex { get { return index; } set { index = value; } }





    // Start is called before the first frame update
    protected override void Start()
    {
        // Initialize the computation field
        if(computeField == null) {
            computeField = GetComponent<VectorField>();
        }
        if(!inField) { // This check doesn't work properly. 
            computeField.enabled = false;
            divRenderer.enabled = false;
        }

        // This should be called before the display function is called. 
        computeField.preDisplay += CalculateDiv;

        // Initialize the divRenderer
        if(divRenderer == null) {
            divRenderer = GetComponent<DivRender>();
        }

        detectorReadout = new FloatReadout("Divergence");

        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if(!inField) { return; }

        computeField.fieldType = detectedField.fieldType;

        divRenderer.divBuffer = divBuffer; // Make sure order of events here is correct
        // Does this need to be set every frame? Maybe just on enter/exit field
    }

    protected override void OnEnable()
    {
        // Initialize the divergence buffer field. 
        unsafe
        {   // Each component of this vector is the partial derivative in one direction. 
            divBuffer = new ComputeBuffer(1, sizeof(Vector3));
        }

        base.OnEnable();
    }

    protected override void OnDisable()
    {
        if(divBuffer != null)
        {
            divBuffer.Release();
            divBuffer = null;
        }

        base.OnDisable();
    }





    /// <summary>
    /// Calculate the divergence and store the value in divergence. 
    /// </summary>
    private void CalculateDiv()
    {
        int kernelID = 0;

        // Calling the compute shader. 
        divergenceComputer.SetBuffer(kernelID, "_Vectors", computeField.vectorsBuffer);
        divergenceComputer.SetBuffer(kernelID, "_Divergence", divBuffer);
        divergenceComputer.SetFloat("_DeltaX", ((DerivativeZone)computeField.zone).deltaX);

        divergenceComputer.Dispatch(kernelID, 1, 1, 1);

        divBuffer.GetData(tempDivArray);

        divergence = tempDivArray[0].x + tempDivArray[0].y + tempDivArray[0].z;

        ((FloatReadout)detectorReadout).output = divergence;

        // Debug Code
        //Debug.Log("Divergence components: " + tempDivArray[0]);
        //Debug.Log("Stored divergence: " + divergence);
    }

    public override void EnteredField(VectorField graph)
    {
        computeField.enabled = true;
        divRenderer.divBuffer = divBuffer;
        divRenderer.enabled = true;
        ((FloatReadout)detectorReadout).isActive = true;
        base.EnteredField(graph);
    }

    public override void ExitedField(VectorField graph)
    {
        computeField.enabled = false;
        divRenderer.enabled = false;
        ((FloatReadout)detectorReadout).isActive = false;
        base.ExitedField(graph);
    }
}
