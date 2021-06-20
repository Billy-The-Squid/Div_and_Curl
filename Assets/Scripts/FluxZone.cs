using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(FluxDetector))]
public class FluxZone : FieldZone
{
    [SerializeField]
    MeshFilter meshFilter;
    Mesh mesh;

    [SerializeField]
    FluxDetector fluxDetector;

    private bool initialized = false;





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
        
        fluxDetector.detectedField.zone.Initialize();
        fieldOrigin = fluxDetector.detectedField.zone.fieldOrigin;

        bounds = new Bounds(mesh.bounds.center, mesh.bounds.size + Vector3.one * maxVectorLength);
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

        initialized = true;
    }
}
