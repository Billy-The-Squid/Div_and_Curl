using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripleCurlZone : FieldZone
{
    [SerializeField, Min(4)]
    public int resolution = 20;
    public float scalingFactor = 2;

    Vector3[] posArray;
    Vector3[] tanArray; // The tangents to the loop.
    public ComputeBuffer tangentBuffer { get; protected set; }

    protected float radius; // The actual radius of the sphere.
    protected bool initialized = false;

    public override void Initialize()
    {
        if(initialized) { return; }

        numberOfPoints = 3 * resolution;
        posArray = new Vector3[numberOfPoints];
        tanArray = new Vector3[numberOfPoints];
        unsafe {
            positionBuffer = new ComputeBuffer(numberOfPoints, sizeof(Vector3));
            tangentBuffer = new ComputeBuffer(numberOfPoints, sizeof(Vector3));
        }
        canMove = true;
        initialized = true;
    }

    private void OnDisable()
    {
        if(positionBuffer != null)
        {
            positionBuffer.Release();
            positionBuffer = null;
        }
        if(tangentBuffer != null)
        {
            tangentBuffer.Release();
            tangentBuffer = null;
        }
        initialized = false;
    }

    public override void SetPositions()
    {
        Initialize();

        radius = 0.5f * transform.localScale.x;
        maxVectorLength = scalingFactor * 2 * Mathf.PI * radius / resolution;
        fieldOrigin = transform.position; // Deal with this somewhere else, I guess? 
        Debug.LogWarning("Still setting fieldOrigin---don't let me do this");
        bounds = new Bounds(transform.position, 2 * (transform.localScale + Vector3.one * maxVectorLength));

        int i;
        // for calculating the x-component
        for (i = 0; i < resolution; i++) {
            posArray[i] = new Vector3(0, Mathf.Sin(2 * Mathf.PI * i/ resolution), Mathf.Cos(2 * Mathf.PI * i / resolution)) * 0.5f;
            posArray[i] = transform.TransformPoint(posArray[i]);
            tanArray[i] = new Vector3(0, Mathf.Cos(2 * Mathf.PI * i / resolution), -Mathf.Sin(2 * Mathf.PI * i / resolution)) * 0.5f;
            tanArray[i] = transform.TransformVector(tanArray[i]);
        }
        // for calculating the y-component
        for(i = resolution; i < 2 * resolution; i++) {
            posArray[i] = new Vector3(Mathf.Cos(2 * Mathf.PI * i / resolution), 0, Mathf.Sin(2 * Mathf.PI * i / resolution)) * 0.5f;
            posArray[i] = transform.TransformPoint(posArray[i]);
            tanArray[i] = new Vector3(-Mathf.Sin(2 * Mathf.PI * i / resolution), 0, Mathf.Cos(2 * Mathf.PI * i / resolution)) * 0.5f;
            tanArray[i] = transform.TransformVector(tanArray[i]);
        }
        // for calculating the z-component
        for(i = 2 * resolution; i < 3 * resolution; i++) {
            posArray[i] = new Vector3(Mathf.Cos(2 * Mathf.PI * i / resolution), -Mathf.Sin(2 * Mathf.PI * i / resolution), 0) * 0.5f;
            posArray[i] = transform.TransformPoint(posArray[i]);
            tanArray[i] = new Vector3(-Mathf.Sin(2 * Mathf.PI * i / resolution), Mathf.Cos(2 * Mathf.PI * i / resolution), 0) * 0.5f;
            tanArray[i] = transform.TransformVector(tanArray[i]);
        }

        positionBuffer.SetData(posArray);
        tangentBuffer.SetData(tanArray);

        Debug.LogWarning("SetPositions not implemented");
    }
}
