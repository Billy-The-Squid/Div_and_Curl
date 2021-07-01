using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VectorField), typeof(Rigidbody), typeof(CurlSpinZone))]
public class CurlSpinDetector : FieldDetector
{
    /// <summary>
    /// The vector curl at the center of the detector.
    /// </summary>
    Vector3 curl;

    /// <summary>
    /// The compute shader used to calculate the curl.
    /// </summary>
    [SerializeField]
    ComputeShader curlComputer;

    /// <summary>
    /// The <cref>VectorField</cref> that directs the field computation
    /// </summary>
    VectorField computationField;

    /// <summary>
    /// The detector's RigidBody.
    /// </summary>
    [SerializeField]
    Rigidbody displayRigidBody;

    /// <summary>
    /// The buffer used to get the value of the curl. 
    /// </summary>
    protected ComputeBuffer curlBuffer;
    // Temporary array
    Vector3[] tempCurlArray = new Vector3[1];






    // Start is called before the first frame update
    void Start()
    {
        // Set up the vector field
        if(computationField == null) {
            computationField = GetComponent<VectorField>();
        }
        if(!inField) {
            computationField.enabled = false; // I hope this is safe...
        }

        // initialize the curl buffer
        unsafe
        {
            curlBuffer = new ComputeBuffer(1, sizeof(Vector3));
        }

        // Find the RigidBody
        if(displayRigidBody == null)
        {
            displayRigidBody = GetComponent<Rigidbody>();
        }

        quantityName = "Curl";
    }

    // Update is called once per frame
    void Update()
    {
        if(!inField) { return; } // What should this detector do when it isn't in a field?

        // These should be attached to preCalculation
        computationField.fieldType = detectedField.fieldType;
        computationField.zone.fieldOrigin = detectedField.zone.fieldOrigin;

        // This should be attached to preDisplay
        CalculateCurl();

        displayRigidBody.angularVelocity = curl; // Scale this so that the visual rate of spin matches the rate that particles will move
    }



    private void OnDisable()
    {
        if(curlBuffer != null)
        {
            curlBuffer.Release();
            curlBuffer = null;
        }
    }



    /// <summary>
    /// Calculates the local curl and assigns that to <cref>curl</cref>
    /// </summary>
    private void CalculateCurl()
    {
        int kernelID = 0;

        curlComputer.SetBuffer(kernelID, "_Vectors", computationField.vectorsBuffer);
        curlComputer.SetBuffer(kernelID, "_Curl", curlBuffer);
        curlComputer.SetFloat("_DeltaX", ((CurlSpinZone)computationField.zone).deltaX);
        // Throw an error or something if this cast doesn't work. 

        curlComputer.Dispatch(kernelID, 1, 1, 1);

        curlBuffer.GetData(tempCurlArray);
        curl = tempCurlArray[0];

        detectorOutput = curl.magnitude;
    }



    public override void EnteredField(VectorField graph)
    {
        computationField.enabled = true; // Welp.
        base.EnteredField(graph);
    }

    public override void ExitedField(VectorField graph)
    {
        computationField.enabled = false;
        base.ExitedField(graph);
        displayRigidBody.angularVelocity = Vector3.zero;
        detectorOutput = (curl = Vector3.zero).magnitude;
    }
}
