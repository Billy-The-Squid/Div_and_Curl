using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CurlLoopZone), typeof(VectorField))]
public class CurlLoopDetector : FieldDetector
{
    /// <summary>
    /// The object that sets the initial positions
    /// </summary>
    [SerializeField]
    protected CurlLoopZone zone;

    /// <summary>
    /// The object that computes the vector field values
    /// </summary>
    [SerializeField]
    protected VectorField localField;

    /// <summary>
    /// The compute shader used to compute the integral
    /// </summary>
    [SerializeField]
    protected ComputeShader integrator;

    /// <summary>
    /// The buffer that stores the magnitude of the curl component. Single-entry.
    /// </summary>
    protected ComputeBuffer curlBuffer;

    /// <summary>
    /// Array storing the value in curlBuffer;
    /// </summary>
    protected float[] curlArray = new float[1];

    /// <summary>
    /// The buffer that stores the contributions to the integral. Float buffer.
    /// </summary>
    protected ComputeBuffer contributionsBuffer;

    /// <summary>
    /// The magnitude of the average curl
    /// </summary>
    protected float averageCurl;

    /// <summary>
    /// The detector's RigidBody.
    /// </summary>
    [SerializeField]
    Rigidbody displayRigidBody;
    ///// <summary>
    ///// The parent of the displayRigidBody. Must be free to rotate
    ///// </summary>
    //[SerializeField]
    //Transform displayParent;





    // Start is called before the first frame update
    void Start()
    {
        // Setting some variables
        if(zone == null) {
            zone = GetComponent<CurlLoopZone>();
        }
        if(localField == null)
        {
            localField = GetComponent<VectorField>();
        }

        localField.enabled = inField;

        quantityName = "Average Curl";

        // Initializing the compute buffers
        contributionsBuffer = new ComputeBuffer(zone.resolution, sizeof(float));
        curlBuffer = new ComputeBuffer(1, sizeof(float));

        //localField.preCalculations += 
    }

    // Update is called once per frame
    void Update()
    {
        Integrate();

        displayRigidBody.angularVelocity = -0.5f * averageCurl * transform.up;
    }

    private void OnDisable()
    {
        if(contributionsBuffer != null)
        {
            contributionsBuffer.Release();
            contributionsBuffer = null;
        }
        if(curlBuffer != null)
        {
            curlBuffer.Release();
            curlBuffer = null;
        }
    }





    private void Integrate()
    {
        if(!inField) { return; }

        zone.fieldOrigin = detectedField.zone.fieldOrigin; // Should be attached to preCalculations?
        localField.fieldType = detectedField.fieldType;

        // Calculate the contributions
        int kernelID = 0;
        integrator.SetBuffer(kernelID, "_Vectors", localField.vectorsBuffer);
        integrator.SetBuffer(kernelID, "_Tangents", zone.tangentBuffer);
        integrator.SetBuffer(kernelID, "_Contributions", contributionsBuffer);
        integrator.SetInt("_NumberOfPoints", zone.resolution);

        // Change the grouping. 
        int numGroups = Mathf.CeilToInt(zone.resolution / 64f);
        integrator.Dispatch(kernelID, numGroups, 1, 1);

        //// Debug Code
        //float[] debugArray = new float[zone.resolution];
        //contributionsBuffer.GetData(debugArray);


        // Total the integral // UNCOMMENT ME
        kernelID = 1;
        integrator.SetBuffer(kernelID, "_Contributions", contributionsBuffer);
        integrator.SetBuffer(kernelID, "_Result", curlBuffer);

        integrator.Dispatch(kernelID, 1, 1, 1);

        // Next: do stuff with these values. 
        curlBuffer.GetData(curlArray);
        averageCurl = curlArray[0] / (Mathf.PI * transform.localScale.x * transform.localScale.y);
        detectorOutput = averageCurl;
        // Won't work for general shapes, but should for circles and ellipses. 
    }

    public override void EnteredField(VectorField graph)
    {
        localField.enabled = true;
        base.EnteredField(graph);
    }

    public override void ExitedField(VectorField graph)
    {
        localField.enabled = false;
        detectorOutput = 0.0f;
        base.ExitedField(graph);
    }
}
