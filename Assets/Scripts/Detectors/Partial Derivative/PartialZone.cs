using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartialZone : FieldZone
{
    public float deltaX;

    protected bool initialized;
    protected Vector3[] points = new Vector3[2];



    public override void Initialize()
    {
        if(initialized) { return; }

        unsafe
        {
            positionBuffer = new ComputeBuffer(2, sizeof(Vector3));
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
    }


    public override void SetPositions()
    {
        Initialize();

        // Set the current positions
        points[0] = transform.right * deltaX; //transform.TransformVector(new Vector3(deltaX, 0, 0));
        points[1] = -transform.right * deltaX; //transform.TransformVector(new Vector3(-deltaX, 0, 0));
        positionBuffer.SetData(points);

        // Set the bounds
        bounds = new Bounds(transform.position, 2 * deltaX * Vector3.one * transform.localScale.x);
    }
}
