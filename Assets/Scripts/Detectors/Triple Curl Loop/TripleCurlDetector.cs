using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripleCurlDetector : FieldDetector
{
    [Header("Detector")]
    public CurlLoopZone zone;
    public VectorField localField;

    public ComputeShader integrator;

    protected ComputeBuffer contributionsBuffer;
    public ComputeBuffer curl; // Single-entry, vector
    public Vector3[] curlArray = new Vector3[1];

    public ComputeBuffer projectionsBuffer;
    public VectorDisplay projectionsDisplay;

    public override string displayName 
    { 
        get => throw new System.NotImplementedException(); 
        set => throw new System.NotImplementedException(); 
    }
    public override string displayDescription 
    { 
        get => throw new System.NotImplementedException(); 
        set => throw new System.NotImplementedException(); 
    }
    public override int menuIndex 
    { 
        get => throw new System.NotImplementedException(); 
        set => throw new System.NotImplementedException(); 
    }



    // Start is called before the first frame update
    protected override void Start()
    {
        // Setting some variables
        if (zone == null)
        {
            zone = GetComponent<CurlLoopZone>();
        }
        if (localField == null)
        {
            localField = GetComponent<VectorField>();
        }

        localField.enabled = inField;

        detectorReadout = new FloatReadout("Curl / Area (Component)");

        // Initializing the compute buffers
        contributionsBuffer = new ComputeBuffer(zone.resolution, sizeof(float));
        curl = new ComputeBuffer(1, sizeof(float));

        localField.preDisplay += Integrate;
        base.Start();
    }

    private void Update()
    {
        // Bind this stuff to preDisplay as well, I think?

        if (projectionsBuffer == null && localField.enabled)
        {
            unsafe
            {
                projectionsBuffer = new ComputeBuffer(localField.vectorsBuffer.count, sizeof(Vector3));
                // I think this has been initialized?
            }
        }

        DisplayAxes();
        DisplayProjections();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        // ContributionsBuffer, curl, and projectionsBuffer

        if(contributionsBuffer != null)
        {
            contributionsBuffer.Release();
            contributionsBuffer = null;
        }
        if(curl != null)
        {
            curl.Release();
            curl = null;
        }
        if(projectionsBuffer != null)
        {
            projectionsBuffer.Release();
            projectionsBuffer = null;
        }


        base.OnDisable();
    }



    protected void Integrate()
    {

    }

    protected void DisplayAxes()
    {

    }

    protected void DisplayProjections ()
    {

    }
}
