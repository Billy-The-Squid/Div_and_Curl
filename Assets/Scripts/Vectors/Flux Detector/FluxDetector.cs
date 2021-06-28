using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The flux detector has two forms of display: its color at each point changes based on the contribution of that point to the flux, 
/// and each vertex displays a "hair" indicating the direction of the field at that point. 
/// </summary>
[RequireComponent(typeof(VectorField), typeof(FluxZone))]
public class FluxDetector : FieldDetector
{
    /// <summary>
    /// The material to be displayed when the detector is inside a vector field. 
    /// </summary>
    [SerializeField]
    Material activeMaterial;
    /// <summary>
    /// The material to be displayed when the detector is not inside a vector field. 
    /// </summary>
    [SerializeField]
    Material inertMaterial;
    /// <summary>
    /// The <cref>meshRenderer</cref> for the flux detector
    /// </summary>
    [SerializeField]
    MeshRenderer meshRenderer;
    /// <summary>
    /// The detector mesh.
    /// </summary>
    Mesh mesh;

    /// <summary>
    /// The <cref>FluxZone</cref> object to be used by the <cref>vectorField</cref>.
    /// </summary>
    [SerializeField]
    FluxZone zone;
    /// <summary>
    /// The <cref>VectorField</cref> that is produced by the detector. Not to be confused with the <cref>detectedField</cref>.
    /// </summary>
    [SerializeField]
    VectorField vectorField;



    /// <summary>
    /// The compute shader that calculates the flux contributions.
    /// </summary>
    [SerializeField]
    ComputeShader computeShader;

    static readonly int
        vectorsID = Shader.PropertyToID("_Vectors"),
        fluxContributionsID = Shader.PropertyToID("_FluxContributions"),
        normalsID = Shader.PropertyToID("_Normals"),
        totalFluxID = Shader.PropertyToID("_TotalFlux");
    /// <summary>
    /// Contains the vectors created by <cref>vectorField</cref>.
    /// </summary>
    ComputeBuffer vectorsBuffer;
    /// <summary>
    /// Contains the normals buffer created by the FluxZone.
    /// </summary>
    ComputeBuffer normalsBuffer;

    /// <summary>
    /// Contains the contribution of each vertex to the flux. 
    /// </summary>
    ComputeBuffer fluxContributions;
    /// <summary>
    /// A buffer used to read out the total flux of the object.
    /// </summary>
    ComputeBuffer totalFluxBuffer;
    /// <summary>
    /// A temporary array. 
    /// </summary>
    float[] totalFluxArray;




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
        meshRenderer.material = inField ? activeMaterial : inertMaterial;

        // Finding the vectorField and zone components
        if (zone == null) {
            zone = GetComponent<FluxZone>();
        }
        if (vectorField == null) {
            vectorField = GetComponent<VectorField>();
        }

        // Enabling/disabling the vector field appropriately. 
        vectorField.enabled = inField;
        vectorField.zone = zone; // Hopefully this is fine for a disabled attribute

        // Initializing the storage array.
        totalFluxArray = new float[1];
    }



    private void Update()
    {
        CalculateFlux();
    }



    private void OnDisable()
    {
        if(fluxContributions != null)
        {
            fluxContributions.Release();
            fluxContributions = null;
        }

        if(totalFluxBuffer != null)
        {
            totalFluxBuffer.Release();
            totalFluxBuffer = null;
        }
    }





    /// <summary>
    /// Everything that should be done while inside a field. 
    /// 
    /// Calculates the flux contributions at each point and sets the material buffers. 
    /// </summary>
    protected void CalculateFlux()
    {
        if (!inField)
        {
            return;
        }

        //if (fluxContributions != null)
        //{ // Don't uncomment this. It's throwing weird errors in the first line. 
        //    float[] debugArray = new float[vectorsBuffer.count];
        //    fluxContributions.GetData(debugArray);
        //    Debug.Log((("First three points in vector array: " + debugArray[0]) + debugArray[1]) + debugArray[2]);
        //    //Debug.Log((("Last three points in vector array: " + debugArray[numOfPoints - 1]) + debugArray[numOfPoints - 2]) + debugArray[numOfPoints - 3]);
        //}

        // Dump all this stuff in a callback from the vector field's preDisplay.
        // It might be best to make a compute shader that will run in the gaps here, 
        // Calculating the flux values at each point and collecting them, then the total. 

        // Makes sure the same field types are being plotted...
        vectorField.fieldType = detectedField.fieldType;

        // Fills the vector and normals buffer
        vectorsBuffer = vectorField.vectorsBuffer;
        // Does this properly sync up?
        normalsBuffer = zone.normalsBuffer;

        // Calculates the flux contributions at each vertex, plus the total flux. 
        if (fluxContributions == null)
        {
            fluxContributions = new ComputeBuffer(vectorsBuffer.count, sizeof(float));
            totalFluxBuffer = new ComputeBuffer(1, sizeof(float));
        }
        CalculateFluxContributions();

        // Sends the vector buffer to the shell shader
        UpdateShellMaterial();

        // Send some information to the pointer material
        UpdatePointerMaterial();
    }



    /// <summary>
    /// Calculates the flux contributions at each vertex and store the resulting values in a computeBuffer. 
    /// Also calculates the flux at each triangle and sums that to the total flux. 
    /// </summary>
    private void CalculateFluxContributions()
    {
        // Calculating the flux contributions
        int kernelID = 0;

        computeShader.SetBuffer(kernelID, vectorsID, vectorsBuffer);
        computeShader.SetBuffer(kernelID, normalsID, normalsBuffer);
        computeShader.SetBuffer(kernelID, fluxContributionsID, fluxContributions);

        int numGroups = Mathf.CeilToInt(vectorsBuffer.count / 64f);
        computeShader.Dispatch(kernelID, numGroups, 1, 1);

        // Calculating the total flux
        kernelID = 1;

        computeShader.SetBuffer(kernelID, fluxContributionsID, fluxContributions);
        computeShader.SetFloat("_NumberOfPoints", vectorsBuffer.count);
        computeShader.SetBuffer(kernelID, totalFluxID, totalFluxBuffer);

        computeShader.Dispatch(kernelID, 1, 1, 1);

        totalFluxBuffer.GetData(totalFluxArray);
    }



    /// <summary>
    /// Sends the vector buffer to the <cref>activeMaterial</cref>.
    /// </summary>
    private void UpdateShellMaterial()
    {
        activeMaterial.SetBuffer(vectorsID, vectorsBuffer);
    }



    /// <summary>
    /// Sends the vector buffer and other information to the pointer material. 
    /// </summary>
    private void UpdatePointerMaterial()
    {
        vectorField.display.pointerMaterial.SetBuffer("_FluxContributions", fluxContributions);

        Vector3 pos = transform.position;
        vectorField.display.pointerMaterial.SetBuffer("_Vectors", vectorsBuffer); // Not done automatically bc normally shader doesn't NEED this buffer. 
        vectorField.display.pointerMaterial.SetVector("_DetectorCenter", new Vector4(pos.x, pos.y, pos.z, 0f));
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
    }
}
