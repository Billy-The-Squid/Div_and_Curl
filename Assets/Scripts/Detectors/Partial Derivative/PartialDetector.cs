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

    public override string displayName { get; set; }
    public override string displayDescription { get; set; }
    public override int menuIndex { get; set; }



    private void Start()
    {
        quantityName = "Directional Derivative";

        computeField.preDisplay += CalculatePartial;
    }

    private void OnEnable()
    {
        if(partialDerivative == null)
        {
            partialDerivative = new ComputeBuffer(1, sizeof(Vector3));
        }
    }


    protected void CalculatePartial()
    {
        int kernelID = 0;
        computeShader.SetBuffer(kernelID, "_Positions", computeField.zone.positionBuffer);
        computeShader.SetBuffer(kernelID, "_Vectors", computeField.vectorsBuffer);
        //computeShader.SetBuffer(kernelID, "_Results", )
    }
}
