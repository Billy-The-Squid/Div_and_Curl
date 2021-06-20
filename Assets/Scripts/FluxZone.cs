using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter), typeof(FluxDetector))]
public class FluxZone : FieldZone
{
    [SerializeField]
    MeshFilter meshFilter;
    Mesh mesh;

    [SerializeField]
    FluxDetector fluxDetector;

    private bool initialized = false;

    private Vector3[] positionArray;





    private void OnEnable() {
        Initialize();
    }



    public override void SetPositions()
    {
        Initialize();
        unsafe {
            if (positionBuffer == null || positionBuffer.count != numberOfPoints || positionBuffer.stride != sizeof(Vector3)) {
                positionBuffer = new ComputeBuffer(numberOfPoints, sizeof(Vector3));
            }
        }

        Vector3 scale = transform.localScale;
        Vector3 position = transform.position;
        Array.Copy(mesh.vertices, positionArray, numberOfPoints);
        for(int i = 0; i < numberOfPoints; i++)
        {
            positionArray[i].x *= scale.x;
            positionArray[i].y *= scale.y;
            positionArray[i].z *= scale.z;
            positionArray[i] += position;
        }
        positionBuffer.SetData(positionArray);

        fluxDetector.detectedField.zone.Initialize();
        fieldOrigin = fluxDetector.detectedField.zone.fieldOrigin; // - transform.position + fluxDetector.detectedField.GetComponent<Transform>().position

        Vector3 boundsCenter = mesh.bounds.center;
        boundsCenter.x *= scale.x;
        boundsCenter.y *= scale.y;
        boundsCenter.z *= scale.z;
        boundsCenter += position;
        bounds = new Bounds(boundsCenter, mesh.bounds.size + Vector3.one * maxVectorLength);
    }

    private void OnDisable()
    {
        if(positionBuffer != null)
        {
            positionBuffer.Release();
            positionBuffer = null;
        }

        initialized = false;
    }

    public override void Initialize()
    {
        if(initialized) { return; }
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }
        mesh = meshFilter.mesh;
        if (fluxDetector == null)
        {
            fluxDetector = GetComponent<FluxDetector>();
        }

        numberOfPoints = mesh.vertexCount;

        positionArray = new Vector3[numberOfPoints];

        initialized = true;
    }
}
