using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// A <cref>FieldZone</cref> that plots vectors across the surface of an object. 
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(FluxDetector))]
public class FluxZone : FieldZone
{
    /// <summary>
    /// The <cref>MeshFilter</cref> containing the mesh to be plotted across. 
    /// </summary>
    [SerializeField]
    MeshFilter meshFilter;
    /// <summary>
    /// The mesh whose vertices are to be used as positions.
    /// </summary>
    Mesh mesh;

    /// <summary>
    /// The main <cref>FluxDetector</cref> object. Used to access the detected field for its origin. 
    /// </summary>
    [SerializeField]
    FluxDetector fluxDetector;

    /// <summary>
    /// Determines whether the field has already been initialized. 
    /// </summary>
    private bool initialized = false;

    /// <summary>
    /// A temporary array used to perform coordinate transformations on vertex positions. 
    /// </summary>
    private Vector3[] positionArray;

    /// <summary>
    /// Multiplied by scale (x component) to calculate the <cref>maxVectorLength</cref>.
    /// </summary>
    [SerializeField]
    float vectorScalingFactor = 0.1f;





    private void OnEnable() {
        Initialize();
    }


    // Fills positionBuffer with values.
    public override void SetPositions()
    {
        // Initializes parameters if not already done. 
        Initialize();
        // creates the positionBuffer if not already done. 
        unsafe {
            if (positionBuffer == null || positionBuffer.count != numberOfPoints || positionBuffer.stride != sizeof(Vector3)) {
                positionBuffer = new ComputeBuffer(numberOfPoints, sizeof(Vector3));
            }
        }

        // Transforms the position data to worldspace and stores it in the buffer. 
        Vector3 scale = transform.localScale;
        Vector3 position = transform.position;
        Array.Copy(mesh.vertices, positionArray, numberOfPoints);
        for(int i = 0; i < numberOfPoints; i++)
        {
            positionArray[i] = transform.TransformPoint(positionArray[i]);
        }
        positionBuffer.SetData(positionArray);

        // Retrieves origin information from the detected field. 
        fluxDetector.detectedField.zone.Initialize();
        fieldOrigin = fluxDetector.detectedField.zone.fieldOrigin;

        // Creates the bounds used by the GPU.
        Vector3 boundsCenter = transform.TransformPoint(mesh.bounds.center);
        bounds = new Bounds(boundsCenter, Vector3.Scale(mesh.bounds.size, transform.localScale) + Vector3.one * maxVectorLength);

        maxVectorLength = transform.localScale.x * vectorScalingFactor;
    }

    private void OnDisable() {
        if(positionBuffer != null) {
            positionBuffer.Release();
            positionBuffer = null;
        }

        initialized = false;
    }

    /// <summary>
    /// Initializes the variables that will not change until the component is disabled,
    /// but which must be created before <cref>SetPositions</cref> may be called. 
    /// </summary>
    public override void Initialize()
    {
        if(initialized) { return; }
        // Initializes some unassigned variables
        if (meshFilter == null) {
            meshFilter = GetComponent<MeshFilter>();
        }
        mesh = meshFilter.mesh;
        if (fluxDetector == null) {
            fluxDetector = GetComponent<FluxDetector>();
        }

        numberOfPoints = mesh.vertexCount;
        positionArray = new Vector3[numberOfPoints];

        maxVectorLength = transform.localScale.x * vectorScalingFactor;

        canMove = true;

        // Ensures that this will not be called again until the component has been disabled and reenabled. 
        initialized = true;
    }
}
