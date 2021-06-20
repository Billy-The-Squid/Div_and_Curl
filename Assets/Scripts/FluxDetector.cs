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
    /// The detector mesh.
    /// </summary>
    Mesh mesh;

    /// <summary>
    /// Contains the vectors created by <cref>vectorField</cref>.
    /// </summary>
    ComputeBuffer vectorsBuffer;
    static readonly int
        vectorsID = Shader.PropertyToID("_Vectors");




    private void OnEnable()
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
        vectorField.zone = zone; // Hopefully this is fine for a disabled attribute.
    }

    private void Update()
    {
        if(!inField)
        {
            return;
        }

        // Makes sure the same fields are being plotted...
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



    /// <summary>
    /// Sends the vector buffer to the <cref>activeMaterial</cref>.
    /// </summary>
    private void UpdateMaterial()
    {
        activeMaterial.SetBuffer(vectorsID, vectorsBuffer);
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
