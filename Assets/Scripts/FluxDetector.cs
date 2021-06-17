using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluxDetector : FieldDetector
{
    [SerializeField]
    ComputeShader computeShader;
    [SerializeField]
    Material activeMaterial, inertMaterial;
    [SerializeField]
    MeshRenderer meshRenderer;

    //[SerializeField]
    Mesh mesh;
    int numVertices;

    ComputeBuffer positionsBuffer, vectorsBuffer;
    static readonly int
        positionsID = Shader.PropertyToID("_Positions"),
        vectorsID = Shader.PropertyToID("_Vectors"),
        centerID = Shader.PropertyToID("_CenterPosition");



    private void OnEnable()
    {
        // Finding the mesh and meshRenderer components
        if (meshRenderer == null) {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        if (mesh == null) {
            mesh = GetComponent<MeshFilter>().mesh;
        }
        numVertices = mesh.vertexCount;

        meshRenderer.material = inField ? activeMaterial : inertMaterial;
        //Debug.Log("Active material? " + (inField ? true : false));

        unsafe
        {
            positionsBuffer = new ComputeBuffer(numVertices, sizeof(Vector3));
            vectorsBuffer = new ComputeBuffer(numVertices, sizeof(Vector3));
        }
    }

    private void OnDisable()
    {
        // release the buffers. 
    }

    private void Update()
    {
        if(!inField)
        {
            return;
        }

        // Sets the vertex data into the position buffer
        positionsBuffer.SetData(mesh.vertices); // Hopefully right

        // Fills the vector buffer
        UpdateGPU();

        // Sends the vector buffer to the shader
        UpdateMaterial();
    }



    // Sends the vector buffer to the shader
    private void UpdateMaterial()
    {
        activeMaterial.SetBuffer(vectorsID, vectorsBuffer);
    }

    // Uses the computeshader to calculate the values of the vectors buffer
    private void UpdateGPU()
    {
        computeShader.SetBuffer(0, positionsID, positionsBuffer);
        computeShader.SetBuffer(0, vectorsID, vectorsBuffer);
        computeShader.SetVector(centerID, field.centerPosition); // Is this right?

        int numGroups = Mathf.CeilToInt(numVertices / 64);
        computeShader.Dispatch(0, numGroups, 1, 1);
    }



    public override void EnteredField(GPUGraph graph)
    {
        meshRenderer.material = activeMaterial;
        base.EnteredField(graph);
    }

    public override void ExitedField(GPUGraph graph)
    {
        meshRenderer.material = inertMaterial;
        base.ExitedField(graph);
    }
}
