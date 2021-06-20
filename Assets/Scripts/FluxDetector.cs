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

    private void Update()
    {
        if(!inField)
        {
            return;
        }

        vectorField.fieldType = detectedField.fieldType;

        // Fills the vector buffer
        vectorsBuffer = vectorField.vectorsBuffer;

        // Sends the vector buffer to the shader
        UpdateMaterial();

        // Send some information to the pointer material
        Vector3 pos = transform.position;
        vectorField.pointerMaterial.SetBuffer("_Vectors", vectorsBuffer);
        vectorField.pointerMaterial.SetVector("_DetectorCenter", new Vector4(pos.x, pos.y, pos.z, 0f));
    }



    // Sends the vector buffer to the shader
    private void UpdateMaterial()
    {
        activeMaterial.SetBuffer(vectorsID, vectorsBuffer);
    }



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
