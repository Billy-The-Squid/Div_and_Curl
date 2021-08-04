using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CurlLoopZone), typeof(VectorField))]
public class CurlLoopDetector : FieldDetector
{
    [Header("Detector")]
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


    // Stuff used for displaying the axis. 
    /// <summary>
    /// Stores the transform position for the axis vector display. Single-entry.
    /// </summary>
    public ComputeBuffer axisPosition { get; protected set; }
    /// <summary>
    /// Stores the axis vector. Single-entry.
    /// </summary>
    public ComputeBuffer axisLength { get; protected set; }
    /// <summary>
    /// The display used to plot the axis vector.
    /// </summary>
    public VectorDisplay axisDisplay;


    /// <summary>
    /// The buffer storing the vector projections.
    /// </summary>
    public ComputeBuffer projectionBuffer { get; protected set; }
    /// <summary>
    /// The Display used to display the projection vectors
    /// </summary>
    public VectorDisplay projectionDisplay;
    // It's not the end of the world if these various displays get mixed up, so long as they're different. 


    private static string nameToDisplay = "Curl (Line Integral)";
    private static string description = "Each component of curl is the line integral of the field dotted with the tangent, divided by the area enclosed.";
    private static int index;

    public override string displayName { get { return nameToDisplay; } set => throw new System.NotImplementedException("I'm not allowing name changing right now."); }
    public override string displayDescription { get { return description; } set => throw new System.NotImplementedException("I'm not allowing description changing right now"); }
    public override int menuIndex { get { return index; } set { index = value; } }





    // Start is called before the first frame update
    protected override void Start()
    {
        // Setting some variables
        if(zone == null) {
            zone = GetComponent<CurlLoopZone>();
        }
        if(localField == null) {
            localField = GetComponent<VectorField>();
        }

        localField.enabled = axisDisplay.enabled = inField;

        detectorReadout = new FloatReadout("Ave. Curl (Component)");

        // Initializing the compute buffers
        contributionsBuffer = new ComputeBuffer(zone.resolution, sizeof(float));
        curlBuffer = new ComputeBuffer(1, sizeof(float));

        // Initializing the buffers for the axis display.
        unsafe {
            axisPosition = new ComputeBuffer(1, sizeof(Vector3));
            axisLength = new ComputeBuffer(1, sizeof(Vector3));
        }

        localField.preDisplay += Integrate;

        base.Start();
    }

    void Update()
    {
        // Bind this stuff to preDisplay as well, I think?

        if(projectionBuffer == null && localField.enabled) {
            unsafe {
                projectionBuffer = new ComputeBuffer(localField.vectorsBuffer.count, sizeof(Vector3));
                // I think this has been initialized?
            }
        }

        displayRigidBody.angularVelocity = 0.5f * averageCurl * transform.forward;

        DisplayAxis();
        DisplayProjections();
    }

    protected override void OnDisable()
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
        if(axisPosition != null)
        {
            axisPosition.Release();
            axisPosition = null;
        }
        if(axisLength != null)
        {
            axisLength.Release();
            axisLength = null;
        }
        if(projectionBuffer != null)
        {
            projectionBuffer.Release();
            projectionBuffer = null;
        }

        base.OnDisable();
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
        integrator.SetBuffer(kernelID, "_Projections", projectionBuffer);
        integrator.SetInt("_NumberOfPoints", zone.resolution);

        // Change the grouping. 
        int numGroups = Mathf.CeilToInt(zone.resolution / 64f);
        integrator.Dispatch(kernelID, numGroups, 1, 1);

        {
            //// Debug Code
            //float[] debugArray = new float[zone.resolution];
            //contributionsBuffer.GetData(debugArray);
        }

        // Total the integral
        kernelID = 1;
        integrator.SetBuffer(kernelID, "_Contributions", contributionsBuffer);
        integrator.SetBuffer(kernelID, "_Result", curlBuffer);

        integrator.Dispatch(kernelID, 1, 1, 1);

        // Next: do stuff with these values. 
        curlBuffer.GetData(curlArray);
        averageCurl = curlArray[0] / (Mathf.PI * transform.localScale.x * transform.localScale.y);
        ((FloatReadout)detectorReadout).output = averageCurl;
        // Won't work for general shapes, but should for circles and ellipses. 
    }



    /// <summary>
    /// Displays a vector with the magnitude of the curl component perpendicular to the loop. 
    /// </summary>
    private void DisplayAxis()
    {
        if (!inField) { return; }

        axisPosition.SetData(new Vector3[1] { transform.position }); // Will this work? I don't know. 
        axisLength.SetData(new Vector3[1] { -transform.forward * averageCurl });

        axisDisplay.maxVectorLength = transform.localScale.x; // This is really arbitrary. 
        axisDisplay.bounds = new Bounds(transform.position, 2 * Vector3.one * transform.localScale.x);
        axisDisplay.DisplayVectors(axisPosition, axisLength);
    }



    /// <summary>
    /// Displays the projections of the vectors onto the tangents.
    /// </summary>
    private void DisplayProjections()
    {
        if(!inField) { return; } 

        if(!projectionDisplay.Initialized)
        {
            projectionDisplay.DisplayVectors(localField.positionsBuffer, projectionBuffer);
        }

        projectionDisplay.maxVectorLength = localField.zone.maxVectorLength;
        projectionDisplay.bounds = localField.zone.bounds;
        projectionDisplay.pointerMaterial.SetBuffer("_CurlContributions", contributionsBuffer);
        projectionDisplay.pointerMaterial.SetBuffer("_Vectors", localField.vectorsBuffer);
        projectionDisplay.pointerMaterial.SetFloat("_dS", 2 * Mathf.PI * transform.localScale.x / zone.resolution);

        {
            // Debug code
            float[] debugArray = new float[contributionsBuffer.count];
            contributionsBuffer.GetData(debugArray);
        }

        projectionDisplay.DisplayVectors(localField.positionsBuffer, projectionBuffer);
    }



    /// <summary>
    /// Sets the color max of the local field to the same as that of the detected field. 
    /// </summary>
    protected void BindColors()
    {
        //var disp = detectedField.display; // Might not work with RainyField?
        //if(disp is VectorDisplay || disp is RainyDisplay)
        //{
        //    ((VectorDisplay)localField.display).RecalculateMaxMagnitude();
        //}
        Debug.LogWarning("This function doesn't make sense anymore. Move this to its own ColorScheme");
    }



    /// <inheritdoc/>
    public override void EnteredField(VectorField graph)
    {
        localField.enabled = true;
        axisDisplay.enabled = true;
        ((FloatReadout)detectorReadout).isActive = true;
        base.EnteredField(graph);
    }

    /// <inheritdoc/>
    public override void ExitedField(VectorField graph)
    {
        localField.enabled = false;
        ((FloatReadout)detectorReadout).isActive = false;
        axisDisplay.enabled = false;
        base.ExitedField(graph);
    }
}
