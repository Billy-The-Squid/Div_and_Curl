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
    Vector3[] worldPositions;
    //Vector3[] debug;



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
            worldPositions = new Vector3[numVertices];
            //debug = new Vector3[numVertices];
        }
    }

    private void OnDisable()
    {
        positionsBuffer.Release();
        vectorsBuffer.Release();

        positionsBuffer = null;
        vectorsBuffer = null;
    }

    private void Update()
    {
        if(!inField)
        {
            return;
        }

        Matrix4x4 matrix = transform.localToWorldMatrix;
        for(int i = 0; i < numVertices; i++)
        {
            worldPositions[i] = matrix.MultiplyPoint3x4(mesh.vertices[i]);
            //worldPositions[i] = transform.TransformPoint(mesh.vertices[i]);
        }
        // Sets the vertex data into the position buffer
        positionsBuffer.SetData(worldPositions); // Hopefully right

        // Fills the vector buffer
        UpdateGPU();
        //vectorsBuffer.GetData(debug);
        //int index = 0; // Mathf.CeilToInt(UnityEngine.Random.Range(0.0f, (float)numVertices) - 1);
        //Debug.Log("Array value " + index + ": " + debug[index]);
        //Debug.Log("World position: " + worldPositions[index]);

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
        computeShader.SetVector(centerID, field.zone.fieldOrigin); // Is this right?
        // Debug.Log("CenterPosition: " + field.centerPosition); // Currently (3, 1.5, 3)

        int numGroups = Mathf.CeilToInt(numVertices / 64);
        computeShader.Dispatch(0, numGroups, 1, 1);
    }



    public override void EnteredField(VectorField field)
    {
        meshRenderer.material = activeMaterial;
        base.EnteredField(field);
    }

    public override void ExitedField(VectorField field)
    {
        meshRenderer.material = inertMaterial;
        base.ExitedField(field);
    }
}
