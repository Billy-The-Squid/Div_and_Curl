using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// A <cref>FieldZone</cref> that plots vectors across the surface of an object. 
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(CurlSphereDetector))]
public class CurlSphereZone : FieldZone
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
    /// The main <cref>CurlDetector</cref> object. Used to access the detected field for its origin. 
    /// </summary>
    [SerializeField]
    CurlSphereDetector curlDetector;

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

    /// <summary>
    /// The compute buffer that stores the normals of each vertex. 
    /// </summary>
    public ComputeBuffer normalsBuffer;

    /// <summary>
    /// A temporary array used to perform coordinate transformations on the normals. 
    /// </summary>
    private Vector3[] normalsArray;





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
            if (normalsBuffer == null || normalsBuffer.count != numberOfPoints || normalsBuffer.stride != sizeof(Vector3))
            {
                normalsBuffer = new ComputeBuffer(numberOfPoints, sizeof(Vector3));
            }
        }

        // Transforms the position data to worldspace and stores it in the buffer. 
        Vector3 scale = transform.localScale; // Is this quantity used?
        Vector3 position = transform.position; // Is this quantity used?
        Array.Copy(mesh.vertices, positionArray, numberOfPoints); // Skip copy and directly transform?
        for(int i = 0; i < numberOfPoints; i++)
        {
            positionArray[i] = transform.TransformPoint(positionArray[i]);
            normalsArray[i] = transform.TransformVector(mesh.normals[i]).normalized;
        }
        positionBuffer.SetData(positionArray);
        normalsBuffer.SetData(normalsArray);

        // Retrieves origin information from the detected field. 
        curlDetector.detectedField.zone.Initialize();
        fieldOrigin = curlDetector.detectedField.zone.fieldOrigin;

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
        if(normalsBuffer != null)
        {
            normalsBuffer.Release();
            normalsBuffer = null;
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
        if (curlDetector == null) {
            curlDetector = GetComponent<CurlSphereDetector>();
        }

        numberOfPoints = mesh.vertexCount;
        positionArray = new Vector3[numberOfPoints]; 
        normalsArray = new Vector3[numberOfPoints];

        maxVectorLength = transform.localScale.x * vectorScalingFactor;

        canMove = true;

        // Ensures that this will not be called again until the component has been disabled and reenabled. 
        initialized = true;
        // Why is the positions buffer not declared here?
    }

    private void OnDrawGizmos()
    {
        // Debug code
        if(positionArray != null && positionArray.Length > 0)
        {
            Gizmos.DrawSphere(positionArray[0], 0.01f);
            Gizmos.DrawLine(positionArray[0], normalsArray[0] + positionArray[0]);

            Gizmos.DrawLine(positionArray[406], normalsArray[406] + positionArray[406]);
            Gizmos.DrawLine(positionArray[494], normalsArray[494] + positionArray[494]);
        }
    }
}
