using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartialZone : FieldZone
{
    protected bool initialized;
    protected Vector3[] points;
    public float deltaX;



    public override void Initialize()
    {
        if(initialized) { return; }

        unsafe
        {
            positionBuffer = new ComputeBuffer(2, sizeof(Vector3));
        }

        canMove = true;

        initialized = true;
        Debug.LogWarning("Initialize not implemented");
    }


    private void OnDisable()
    {
        if(positionBuffer != null)
        {
            positionBuffer.Release();
            positionBuffer = null;
        }
    }


    public override void SetPositions()
    {
        Initialize();

        // Set the current positions
        points[0] = transform.TransformVector(new Vector3(deltaX, 0, 0));
        points[1] = transform.TransformVector(new Vector3(-deltaX, 0, 0));
        positionBuffer.SetData(points);

        // Set the bounds
        bounds = new Bounds(transform.position, 2 * deltaX * Vector3.one * transform.localScale.x);

        Debug.LogWarning("SetPositions not implemented");
    }
}
