using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VectorField), typeof(FluxZone))]
public class FluxDetector : FieldDetector
{
    [SerializeField]
    ComputeShader computeShader;
    [SerializeField]
    Material activeMaterial, inertMaterial;
    [SerializeField]
    MeshRenderer meshRenderer;
    [SerializeField]
    FluxZone zone;
    // The vector field that WILL be produced
    [SerializeField]
    VectorField vectorField;

    Mesh mesh;
    int numVertices;

    ComputeBuffer positionsBuffer, vectorsBuffer;
    static readonly int
        positionsID = Shader.PropertyToID("_Positions"),
        vectorsID = Shader.PropertyToID("_Vectors"),
        centerID = Shader.PropertyToID("_CenterPosition");
    Vector3[] worldPositions;




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

        if (zone == null) {
            zone = GetComponent<FluxZone>();
        }
        if (vectorField == null) {
            vectorField = GetComponent<VectorField>();
        }

        vectorField.enabled = inField;
        vectorField.zone = zone; // Hopefully this is fine for a disabled attribute.

        unsafe
        {
            //positionsBuffer = new ComputeBuffer(numVertices, sizeof(Vector3));
            //vectorsBuffer = new ComputeBuffer(numVertices, sizeof(Vector3));
            // worldPositions = new Vector3[numVertices];
            //debug = new Vector3[numVertices];
        }
    }

    private void LateUpdate()
    {
        if(!inField)
        {
            return;
        }



        //Matrix4x4 matrix = transform.localToWorldMatrix;
        //for(int i = 0; i < numVertices; i++)
        //{
        //    worldPositions[i] = matrix.MultiplyPoint3x4(mesh.vertices[i]);
        //    //worldPositions[i] = transform.TransformPoint(mesh.vertices[i]);
        //}
        //// Sets the vertex data into the position buffer
        //positionsBuffer.SetData(worldPositions); // Hopefully right

        // Fills the vector buffer
        //UpdateGPU();
        //vectorsBuffer.GetData(debug);
        vectorsBuffer = vectorField.vectorsBuffer;
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
    // Can be disposed of?
    //private void UpdateGPU()
    //{
    //    int kernelID = (int)field.fieldType;
    //    computeShader.SetBuffer(kernelID, positionsID, positionsBuffer);
    //    computeShader.SetBuffer(kernelID, vectorsID, vectorsBuffer);
    //    computeShader.SetVector(centerID, field.zone.fieldOrigin); // Is this right?
    //    // Debug.Log("CenterPosition: " + field.centerPosition); // Currently (3, 1.5, 3)

    //    int numGroups = Mathf.CeilToInt(numVertices / 64);
    //    computeShader.Dispatch(kernelID, numGroups, 1, 1);
    //}



    public override void EnteredField(VectorField field)
    {
        meshRenderer.material = activeMaterial;
        base.EnteredField(field);
        vectorField.enabled = true;
    }

    public override void ExitedField(VectorField field)
    {
        meshRenderer.material = inertMaterial;
        base.ExitedField(field);
        vectorField.enabled = false;
    }
}
