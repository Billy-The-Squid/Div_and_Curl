using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The flux detector has two forms of display: its color at each point changes based on the contribution of that point to the flux, 
/// and each vertex displays a "hair" indicating the direction of the field at that point. 
/// </summary>
[RequireComponent(typeof(VectorField), typeof(CurlSphereZone))]
public class CurlSphereDetector : FieldDetector
{
    /// <summary>
    /// The material to be displayed when the detector is inside a vector field. 
    /// </summary>
    [NonSerialized]
    public Material activeMaterial;
    /// <summary>
    /// The material to be displayed when the detector is not inside a vector field. 
    /// </summary>
    [SerializeField]
    Material inertMaterial;
    /// <summary>
    /// The <see cref="Shader"/> used to create the <see cref="activeMaterial"/>.
    /// </summary>
    public Shader activeShader;

    /// <summary>
    /// The <see cref="meshRenderer"/> for the curl detector
    /// </summary>
    [SerializeField]
    MeshRenderer meshRenderer;
    /// <summary>
    /// The detector mesh.
    /// </summary>
    Mesh mesh;

    /// <summary>
    /// The <cref>CurlZone</cref> object to be used by the <cref>vectorField</cref>.
    /// </summary>
    [SerializeField]
    CurlSphereZone zone;
    /// <summary>
    /// The <cref>VectorField</cref> that is produced by the detector. Not to be confused with the <cref>detectedField</cref>.
    /// </summary>
    [SerializeField]
    VectorField vectorField;



    /// <summary>
    /// The compute shader that calculates the curl contributions.
    /// </summary>
    [SerializeField]
    ComputeShader computeShader;

    static readonly int
        vectorsID = Shader.PropertyToID("_Vectors"),
        CurlContributionsID = Shader.PropertyToID("_CurlContributions"),
        normalsID = Shader.PropertyToID("_Normals"),
        totalCurlID = Shader.PropertyToID("_TotalCurl");
    /// <summary>
    /// Contains the vectors created by <cref>vectorField</cref>.
    /// </summary>
    ComputeBuffer vectorsBuffer;
    /// <summary>
    /// Contains the normals buffer created by the CurlZone.
    /// </summary>
    ComputeBuffer normalsBuffer;

    /// <summary>
    /// Contains the contribution of each vertex to the curl. 
    /// </summary>
    ComputeBuffer curlContributions;
    /// <summary>
    /// A buffer used to read out the total curl of the object.
    /// </summary>
    ComputeBuffer totalCurlBuffer;
    /// <summary>
    /// A temporary array. 
    /// </summary>
    Vector3[] totalCurlArray;
    // RETYPED
    /// <summary>
    /// The total curl through the surface. 
    /// </summary>
    public Vector3 totalCurl { get; protected set; }
    // RETYPE

    /// <summary>
    /// A computeBuffer that stores the mesh triangles. 
    /// </summary>
    ComputeBuffer trianglesBuffer;
    // This should really be set inside the curl zone. 
    ComputeBuffer areasBuffer;
    ComputeBuffer numTrianglesPerVertBuffer;

    // A debugging array.
    Vector3[] debugArray;

    /// <summary>
    /// Stores the cross products from the calculations at each vertex.
    /// </summary>
    ComputeBuffer projectionsBuffer;
    /// <summary>
    /// Used to draw the projections
    /// </summary>
    public VectorDisplay projectionDisplay;



    private static string nameToDisplay = "Curl (Surface Integral)";
    private static string description = "A thing. What does it do?";
    private static int index;

    public override string displayName { get { return nameToDisplay; } set => throw new System.NotImplementedException("I'm not allowing name changing right now."); }
    public override string displayDescription { get { return description; } set => throw new System.NotImplementedException("I'm not allowing description changing right now"); }
    public override int menuIndex { get { return index; } set { index = value; } }





    private void Start()
    {
        // Finding the mesh and meshRenderer components
        if (meshRenderer == null) {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        if (mesh == null) {
            mesh = GetComponent<MeshFilter>().mesh;
        }

        // Setting the starting material
        if(activeMaterial == null || activeMaterial.shader != activeShader)
        {
            activeMaterial = new Material(activeShader);
        }
        meshRenderer.material = inField ? activeMaterial : inertMaterial;

        // Finding the vectorField and zone components
        if (zone == null) {
            zone = GetComponent<CurlSphereZone>();
        }
        if (vectorField == null) {
            vectorField = GetComponent<VectorField>();
        }

        // Enabling/disabling the vector field appropriately. 
        vectorField.enabled = inField;
        vectorField.zone = zone; // Hopefully this is fine for a disabled attribute

        // Initializing the storage array.
        totalCurlArray = new Vector3[1];
        // RETYPE

        quantityName = "Curl / V"; // Is this what we want to call it?
    }



    private void Update()
    {
        CalculateCurl();
    }



    private void OnDisable()
    {
        if(curlContributions != null)
        {
            curlContributions.Release();
            curlContributions = null;
        }

        if(totalCurlBuffer != null)
        {
            totalCurlBuffer.Release();
            totalCurlBuffer = null;
        }

        if(trianglesBuffer != null)
        {
            trianglesBuffer.Release();
            trianglesBuffer = null;
        }

        if(areasBuffer != null)
        {
            areasBuffer.Release();
            areasBuffer = null;
        }

        if(numTrianglesPerVertBuffer != null)
        {
            numTrianglesPerVertBuffer.Release();
            numTrianglesPerVertBuffer = null;
        }

        if(projectionsBuffer != null)
        {
            projectionsBuffer.Release();
            projectionsBuffer = null;
        }
    }





    /// <summary>
    /// Everything that should be done while inside a field. 
    /// 
    /// Calculates the curl contributions at each point and sets the material buffers. 
    /// </summary>
    protected void CalculateCurl()
    {
        if (!inField)
        {
            return;
        }

        {
            //if (curlContributions != null)
            //{
            //    if (debugArray == null)
            //    {
            //        debugArray = new Vector3[vectorsBuffer.count];
            //    }
            //    // For some reason, this gets messed up the second time we put it through the array.
            //    curlContributions.GetData(debugArray);
            //    Debug.Log((("First three points in contributions array: " + debugArray[0] + ", ") + debugArray[1] + ", ") + debugArray[2]);
            //    //Debug.Log((("Last three points in vector array: " + debugArray[numOfPoints - 1]) + debugArray[numOfPoints - 2]) + debugArray[numOfPoints - 3]);
            //}
        }

        // Makes sure the same field types are being plotted...
        vectorField.fieldType = detectedField.fieldType;

        // Fills the vector and normals buffer
        vectorsBuffer = vectorField.vectorsBuffer;
        // Does this properly sync up?
        normalsBuffer = zone.normalsBuffer;

        // Calculates the curl contributions at each vertex, plus the total curl. 
        if (curlContributions == null)
        {
            unsafe
            {
                curlContributions = new ComputeBuffer(vectorsBuffer.count, sizeof(Vector3)); // RETYPED
                totalCurlBuffer = new ComputeBuffer(1, sizeof(Vector3)); // RETYPED
            }
        }
        if (trianglesBuffer == null)
        {
            trianglesBuffer = new ComputeBuffer(mesh.triangles.Length, sizeof(int));
            trianglesBuffer.SetData(mesh.triangles);

            {
                //// Debug code
                //List<int> debugTriangles = new List<int>();
                //for (int i = 0; i < mesh.triangles.Length; i++)
                //{
                //    if (mesh.triangles[i] == 0 || mesh.triangles[i] == 406 || mesh.triangles[i] == 494)
                //    {
                //        debugTriangles.Add(i);
                //    }
                //}
                //;
            }
        }
        if (areasBuffer == null)
        {
            areasBuffer = new ComputeBuffer(vectorsBuffer.count, sizeof(float));
        }
        areasBuffer.SetData(new float[vectorsBuffer.count]);
        if (numTrianglesPerVertBuffer == null)
        {
            numTrianglesPerVertBuffer = new ComputeBuffer(vectorsBuffer.count, sizeof(int));
        }
        numTrianglesPerVertBuffer.SetData(new int[vectorsBuffer.count]);
        if (projectionsBuffer == null)
        {
            unsafe
            {
                projectionsBuffer = new ComputeBuffer(vectorsBuffer.count, sizeof(Vector3));
            }
        }
        CalculateCurlContributions();

        // Sends the vector buffer to the shell shader
        UpdateShellMaterial();

        // Send some information to the pointer material
        UpdatePointerMaterial();
    }



    /// <summary>
    /// Calculates the curl contributions at each vertex and store the resulting values in a computeBuffer. 
    /// Also calculates the curl at each triangle and sums that to the total curl. 
    /// </summary>
    private void CalculateCurlContributions()
    {
        // Calculating the curl contributions
        int kernelID = 0;

        {
            //// Debug code.
            //Vector3[] debugArray = new Vector3[vectorsBuffer.count];
            ////float[] debugArray = new float[numOfPoints];
            //normalsBuffer.GetData(debugArray);
            //Debug.Log((("First three points in normals array: " + debugArray[0]) + debugArray[1]) + debugArray[2]);
            //Debug.Log((("Last three points in normals array: " + debugArray[vectorsBuffer.count - 1]) + debugArray[vectorsBuffer.count - 2]) + debugArray[vectorsBuffer.count - 3]);
        }

        computeShader.SetBuffer(kernelID, vectorsID, vectorsBuffer);
        computeShader.SetBuffer(kernelID, normalsID, normalsBuffer);
        computeShader.SetBuffer(kernelID, "_Projections", projectionsBuffer);
        computeShader.SetBuffer(kernelID, CurlContributionsID, curlContributions);

        int numGroups = Mathf.CeilToInt(vectorsBuffer.count / 64f);
        computeShader.Dispatch(kernelID, numGroups, 1, 1);

        // Calculating the total curl
        kernelID = 1;

        computeShader.SetBuffer(kernelID, CurlContributionsID, curlContributions);
        computeShader.SetInt("_NumberOfPoints", vectorsBuffer.count);
        computeShader.SetBuffer(kernelID, totalCurlID, totalCurlBuffer);
        //Debug.Log("Number of points: " + vectorsBuffer.count); // 515 points on a standard sphere?

        // Stuff for triangles method:
        computeShader.SetBuffer(kernelID, "_Triangles", trianglesBuffer);
        computeShader.SetBuffer(kernelID, "_Positions", vectorField.positionsBuffer);
        computeShader.SetInt("_NumberOfTriangles", (int) (mesh.triangles.Length / 3));
        computeShader.SetBuffer(kernelID, "_Areas", areasBuffer);
        computeShader.SetBuffer(kernelID, "_NumberOfTrianglesPerVertex", numTrianglesPerVertBuffer);

        {
            //// More debug code (for triangles)
            //Vector3[] debugArray2 = new Vector3[vectorsBuffer.count];
            ////float[] debugArray2 = new float[numOfPoints];
            //vectorField.positionsBuffer.GetData(debugArray2);
            //Debug.Log((("First three points in normals array: " + debugArray2[0]) + debugArray2[1]) + debugArray2[2]);
            //Debug.Log((("Last three points in normals array: " + debugArray2[vectorsBuffer.count - 1]) + debugArray2[vectorsBuffer.count - 2]) + debugArray2[vectorsBuffer.count - 3]);
        }

        computeShader.Dispatch(kernelID, 1, 1, 1);

        totalCurlBuffer.GetData(totalCurlArray);
        totalCurl = totalCurlArray[0];

        detectorOutput = totalCurl.magnitude / (4f / 3 * Mathf.PI * 1 / 8f * transform.localScale.x * transform.localScale.y * transform.localScale.z);

        //Debug.Log("Total curl / V: " + totalCurl / (4f / 3 * Mathf.PI * 1 / 8f * transform.localScale.x * transform.localScale.y * transform.localScale.z));
    }



    /// <summary>
    /// Sends the vector buffer to the <cref>activeMaterial</cref>.
    /// 
    /// Should link to CurlUnlit.shader.
    /// </summary>
    private void UpdateShellMaterial()
    {
        activeMaterial.SetBuffer(CurlContributionsID, curlContributions);
    }



    /// <summary>
    /// Sends the vector buffer and other information to the pointer material. 
    /// 
    /// Should link to CurlShader.shader.
    /// </summary>
    private void UpdatePointerMaterial()
    {
        if(!projectionDisplay.Initialized)
        {
            projectionDisplay.DisplayVectors(vectorField.positionsBuffer, projectionsBuffer);
        }

        projectionDisplay.pointerMaterial.SetBuffer("_CurlContributions", curlContributions);

        projectionDisplay.maxVectorLength = zone.maxVectorLength;
        projectionDisplay.bounds = zone.bounds;
        projectionDisplay.DisplayVectors(vectorField.positionsBuffer, projectionsBuffer);

        {
            //Vector3 pos = transform.position;
            //vectorField.display.pointerMaterial.SetBuffer("_Vectors", vectorsBuffer); // Not done automatically bc normally shader doesn't NEED this buffer. 
            //vectorField.display.pointerMaterial.SetVector("_DetectorCenter", new Vector4(pos.x, pos.y, pos.z, 0f));
            //// Should the last value of this be 1f?
        }
    }



    // Changes the material and enables the vector field. 
    public override void EnteredField(VectorField field)
    {
        meshRenderer.material = activeMaterial;
        base.EnteredField(field);
        vectorField.enabled = true;
    }



    // Changes the material and disables the vector field. 
    public override void ExitedField(VectorField field)
    {
        meshRenderer.material = inertMaterial;
        base.ExitedField(field);
        vectorField.enabled = false;
        detectorOutput = 0.0f;
    }
}
