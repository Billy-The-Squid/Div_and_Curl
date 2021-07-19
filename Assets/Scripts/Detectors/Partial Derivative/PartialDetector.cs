using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartialDetector : FieldDetector
{
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
    public Vector3[] tempArray = new Vector3[1];

    /// <summary>
    /// The component used to display the derivative.
    /// </summary>
    public PartialRenderer partialRenderer;

    public override string displayName { get; set; }
    public override string displayDescription { get; set; }
    public override int menuIndex { get; set; }



    private void Start()
    {
        quantityName = "Directional Derivative";

        computeField.preDisplay += CalculatePartial;
    }

    private void Update()
    {
        if(!inField) { return; }

        computeField.fieldType = detectedField.fieldType;
        computeField.zone.fieldOrigin = detectedField.zone.fieldOrigin;
    }

    private void OnEnable()
    {
        if(partialDerivative == null)
        {
            unsafe
            {
                partialDerivative = new ComputeBuffer(1, sizeof(Vector3));
            }
        }
    }

    protected void OnDisable()
    {
        if(partialDerivative != null)
        {
            partialDerivative.Release();
            partialDerivative = null;
        }
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
        detectorOutput = tempArray[0].magnitude;
    }

    public override void EnteredField(VectorField graph)
    {
        computeField.enabled = true;
        partialRenderer.partialDerivative = partialDerivative;
        partialRenderer.enabled = false;
        base.EnteredField(graph);
    }

    public override void ExitedField(VectorField graph)
    {
        computeField.enabled = false;
        partialRenderer.enabled = false;
        base.ExitedField(graph);
        detectorOutput = 0;
    }
}
