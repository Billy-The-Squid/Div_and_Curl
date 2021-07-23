using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartialDetector : FieldDetector
{
    [Header("Detector")]
    [Tooltip("The field used to carry out the computations")]
    [SerializeField]
    protected VectorField computeField;

    /// <summary>
    /// The compute shader used to find the derivative.
    /// </summary>
    [Tooltip("The compute shader used to find the derivative"), SerializeField]
    protected ComputeShader computeShader;

    /// <summary>
    /// The partial derivative of the field calculated this frame. Single-entry.
    /// </summary>
    public ComputeBuffer partialDerivative;
    [System.NonSerialized]
    public Vector3[] tempArray = new Vector3[1];

    /// <summary>
    /// The component used to display the derivative.
    /// </summary>
    public PartialRenderer partialRenderer;

    public override string displayName { get; set; }
    public override string displayDescription { get; set; }
    public override int menuIndex { get; set; }



    protected override void Start()
    {
        detectorReadout = new VectorReadout("Directional Derivative");
        
        computeField.preDisplay += CalculatePartial;

        base.Start();
    }

    private void Update()
    {
        if(!inField) { return; }

        computeField.fieldType = detectedField.fieldType;
        computeField.zone.fieldOrigin = detectedField.zone.fieldOrigin;
    }

    protected override void OnEnable()
    {
        if(partialDerivative == null)
        {
            unsafe
            {
                partialDerivative = new ComputeBuffer(1, sizeof(Vector3));
            }
        }

        base.OnEnable();
    }

    protected override void OnDisable()
    {
        if(partialDerivative != null)
        {
            partialDerivative.Release();
            partialDerivative = null;
        }

        base.OnDisable();
    }


    protected void CalculatePartial()
    {
        // Compute the partial derivative
        int kernelID = 0;
        computeShader.SetFloat("_DeltaX", ((PartialZone)computeField.zone).deltaX);
        computeShader.SetBuffer(kernelID, "_Vectors", computeField.vectorsBuffer);
        computeShader.SetBuffer(kernelID, "_Results", partialDerivative);
        computeShader.Dispatch(kernelID, 1, 1, 1);

        partialDerivative.GetData(tempArray);
        ((VectorReadout)detectorReadout).output = tempArray[0];

        {
            //// Debug code
            ////Debug.Log("partial derivative: " + tempArray[0]);
            //Vector3[] debugArray = new Vector3[2];
            //computeField.vectorsBuffer.GetData(debugArray);
            //Debug.Log("Vector 0: " + debugArray[0]);
        }

        // Tell the renderer to get to work 
        partialRenderer.partialDerivative = partialDerivative;
        partialRenderer.bounds = computeField.zone.bounds;
        partialRenderer.CreateDisplay();
        // There's got to be a better place to call this & still ensure proper ordering
    }

    public override void EnteredField(VectorField graph)
    {
        computeField.enabled = true;
        partialRenderer.partialDerivative = partialDerivative;
        partialRenderer.enabled = true;
        ((VectorReadout)detectorReadout).isActive = true;
        base.EnteredField(graph);
    }

    public override void ExitedField(VectorField graph)
    {
        computeField.enabled = false;
        partialRenderer.enabled = false;
        base.ExitedField(graph);
        ((VectorReadout)detectorReadout).isActive = false;
    }
}
