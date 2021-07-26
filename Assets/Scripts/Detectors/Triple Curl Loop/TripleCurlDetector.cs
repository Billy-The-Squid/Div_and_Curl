using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripleCurlDetector : FieldDetector
{
    [Header("Detector")]
    public TripleCurlZone zone;
    public VectorField localField;

    public ComputeShader integrator;

    protected ComputeBuffer contributionsBuffer;
    /// <summary>
    /// Four entries. The first three are the x, y, and z components of curl, 
    /// the fourth is the net curl. 
    /// </summary>
    public ComputeBuffer curl;
    [System.NonSerialized]
    public Vector3[] curlArray;

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
            zone = GetComponent<TripleCurlZone>();
        }
        if (localField == null)
        {
            localField = GetComponent<VectorField>();
        }
        if(localField.zone != zone)
        {
            Debug.LogError("LocalField zone and TripleCurlZone are not set to the same instance.");
        }

        localField.enabled = inField;

        detectorReadout = new VectorReadout("Curl / Area (Component)");

        // Initializing the compute buffers
        contributionsBuffer = new ComputeBuffer(zone.resolution, sizeof(float));
        unsafe
        {
            curl = new ComputeBuffer(4, sizeof(Vector3));
        }
        curlArray = new Vector3[4];

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
        if(!inField) { return; }

        localField.zone.fieldOrigin = detectedField.zone.fieldOrigin;
        localField.fieldType = detectedField.fieldType;

        integrator.SetInt("_Resolution", zone.resolution);

        // Calculate the contributions at each point
        int kernelID = 0;

        integrator.SetBuffer(kernelID, "_Vectors", localField.vectorsBuffer);
        integrator.SetBuffer(kernelID, "_Tangents", ((TripleCurlZone)localField.zone).tangentBuffer);
        integrator.SetBuffer(kernelID, "_Contributions", contributionsBuffer);
        integrator.SetBuffer(kernelID, "_Projections", projectionsBuffer);

        int numGroups = Mathf.CeilToInt(zone.numberOfPoints / 64f);
        integrator.Dispatch(kernelID, numGroups, 1, 1);

        // Calculate the total curl from that
        kernelID = 1;

        integrator.SetBuffer(kernelID, "_Contributions", contributionsBuffer);
        integrator.SetBuffer(kernelID, "_Curl", curl);

        integrator.Dispatch(kernelID, 1, 1, 1);

        // Use the information.
        curl.GetData(curlArray);
        Matrix4x4 Areas = Matrix4x4.Scale(new Vector3(1 / (transform.localScale.y * transform.localScale.z),
            1 / (transform.localScale.x * transform.localScale.z),
            1 / (transform.localScale.x * transform.localScale.y)));
        ((VectorReadout)detectorReadout).output = Areas.MultiplyVector(curlArray[3]);

        Debug.Log("Using unverified coordinate transformation");
        Debug.Log("Curl: " + Areas.MultiplyVector(curlArray[3]));

        Debug.Log(((("Array: " + curlArray[0]) + curlArray[1]) + curlArray[2]) + curlArray[3]);
    }

    protected void DisplayAxes()
    {

    }

    protected void DisplayProjections ()
    {

    }

    public override void EnteredField(VectorField graph)
    {
        localField.enabled = true;
        ((VectorReadout)detectorReadout).isActive = true;
        // Also set the displays to true. 
        base.EnteredField(graph);
    }

    public override void ExitedField(VectorField graph)
    {
        localField.enabled = false;
        ((VectorReadout)detectorReadout).isActive = false;
        // Also set the displays to false;
        base.ExitedField(graph);
    }
}
