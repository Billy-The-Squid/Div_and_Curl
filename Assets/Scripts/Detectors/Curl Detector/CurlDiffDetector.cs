using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VectorField), typeof(DerivativeZone), typeof(CurlRenderer))]
public class CurlDiffDetector : FieldDetector
{
    /// <summary>
    /// The compute shader used to calculate the curl.
    /// </summary>
    [Header("Detector")]
    [SerializeField]
    ComputeShader curlComputer;

    /// <summary>
    /// The vector curl at the center of the detector.
    /// </summary>
    Vector3 curl;

    /// <summary>
    /// The <cref>VectorField</cref> that directs the field computation
    /// </summary>
    VectorField computationField;

    /// <summary>
    /// The buffer used to get the components of the curl. 
    /// </summary>
    protected ComputeBuffer curlBuffer;
    // Temporary array
    Vector3[] tempCurlArray = new Vector3[3];

    /// <summary>
    /// The script in charge of displaying the curl.
    /// </summary>
    [SerializeField]
    protected CurlRenderer curlRenderer;

    private static string nameToDisplay = "Curl (Differential)";
    private static string description = "A thing. What does it do?";
    private static int index;

    public override string displayName { get { return nameToDisplay; } set => throw new System.NotImplementedException("I'm not allowing name changing right now."); }
    public override string displayDescription { get { return description; } set => throw new System.NotImplementedException("I'm not allowing description changing right now"); }
    public override int menuIndex { get { return index; } set { index = value; } }






    // Start is called before the first frame update
    protected override void Start()
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
            curlBuffer = new ComputeBuffer(3, sizeof(Vector3));
            //Debug.LogError("Set the curl buffer correctly");
        }

        if(curlRenderer == null)
        {
            curlRenderer = GetComponent<CurlRenderer>();
        }

        detectorReadout = new VectorReadout("Curl");

        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if(!inField) { return; } // What should this detector do when it isn't in a field?

        // These should be attached to preCalculation
        computationField.fieldType = detectedField.fieldType; // Why does this line throw null exceptions?
        computationField.zone.fieldOrigin = detectedField.zone.fieldOrigin;

        // This should be attached to preDisplay
        CalculateCurl();

        curlRenderer.curlBuffer = curlBuffer;
    }



    protected override void OnDisable()
    {
        if(curlBuffer != null)
        {
            curlBuffer.Release();
            curlBuffer = null;
        }

        base.OnDisable();
    }



    /// <summary>
    /// Calculates the local curl and assigns that to <cref>curl</cref>
    /// </summary>
    private void CalculateCurl()
    {
        int kernelID = 0;

        curlComputer.SetBuffer(kernelID, "_Vectors", computationField.vectorsBuffer);
        curlComputer.SetBuffer(kernelID, "_Curl", curlBuffer);
        curlComputer.SetFloat("_DeltaX", ((DerivativeZone)computationField.zone).deltaX);
        // Throw an error or something if this cast doesn't work. 

        curlComputer.Dispatch(kernelID, 1, 1, 1);
        //Debug.LogError("Dispatch the threads correctly");

        curlBuffer.GetData(tempCurlArray);
        curl = new Vector3(0, 0, 0);
        curl.x = tempCurlArray[2].y - tempCurlArray[1].z;
        curl.y = tempCurlArray[0].z - tempCurlArray[2].x;
        curl.z = tempCurlArray[1].x - tempCurlArray[0].y;

        ((VectorReadout)detectorReadout).output = curl;
    }



    public override void EnteredField(VectorField graph)
    {
        computationField.enabled = true;
        curlRenderer.curlBuffer = curlBuffer;
        curlRenderer.enabled = true;
        ((VectorReadout)detectorReadout).isActive = true;
        base.EnteredField(graph);
    }

    public override void ExitedField(VectorField graph)
    {
        computationField.enabled = false;
        curlRenderer.enabled = false;
        base.ExitedField(graph);
        ((VectorReadout)detectorReadout).isActive = false;
        //displayRigidBody.angularVelocity = Vector3.zero;
        curl = Vector3.zero;
    }
}
