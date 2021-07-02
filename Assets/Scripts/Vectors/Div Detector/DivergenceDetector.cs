using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(VectorField), typeof(DerivativeZone))]
public class DivergenceDetector : FieldDetector
{
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





    // Start is called before the first frame update
    void Start()
    {
        // Initialize the computation field
        if(computeField == null) {
            computeField = GetComponent<VectorField>();
        }
        if(!inField) {
            computeField.enabled = false;
        }

        // This should be called before the display function is called. 
        computeField.preDisplay += CalculateDiv;

        // Initialize the divergence buffer field. 
        unsafe
        {
            divBuffer = new ComputeBuffer(1, sizeof(Vector3));
        }

        quantityName = "Div";
    }

    // Update is called once per frame
    void Update()
    {
        if(!inField) { return; }

        //CalculateDiv(); // Bind this to preDisplay
    }

    private void OnDisable()
    {
        if(divBuffer != null)
        {
            divBuffer.Release();
            divBuffer = null;
        }
    }





    /// <summary>
    /// Calculate the divergence and store the value in divergence. 
    /// </summary>
    private void CalculateDiv()
    {
        Debug.Log("CalculateDiv is being called.");

        int kernelID = 0;

        // Calling the compute shader. 
        divergenceComputer.SetBuffer(kernelID, "_Vectors", computeField.vectorsBuffer);
        divergenceComputer.SetBuffer(kernelID, "_Divergence", divBuffer);
        divergenceComputer.SetFloat("_DeltaX", ((DerivativeZone)computeField.zone).deltaX);

        divergenceComputer.Dispatch(kernelID, 1, 1, 1);

        divBuffer.GetData(tempDivArray);

        divergence = tempDivArray[0].x * tempDivArray[0].x + tempDivArray[0].y * tempDivArray[0].y +
            tempDivArray[0].z * tempDivArray[0].z;

        detectorOutput = divergence;

        // Debug Code
        Debug.Log("Divergence components: " + tempDivArray[0]);
        Debug.Log("Stored divergence: " + divergence);
    }

    public override void EnteredField(VectorField graph)
    {
        computeField.enabled = true;
        // Insert something to control the display...
        base.EnteredField(graph);
    }

    public override void ExitedField(VectorField graph)
    {
        computeField.enabled = false;
        // Insert something to control the display...
        detectorOutput = 0;
        base.ExitedField(graph);
    }
}
