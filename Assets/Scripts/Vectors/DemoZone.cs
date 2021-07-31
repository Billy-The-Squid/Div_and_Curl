using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoZone : FieldZone
{
    public DetectorSelector detectorSelector;
    protected bool initialized = false;

    public override void Initialize()
    {
        if(initialized) { return; }
        unsafe
        {
            positionBuffer = new ComputeBuffer(1, sizeof(Vector3));
        }

        canMove = false;
        fieldOrigin = transform.position + new Vector3(0.5f, 0.5f, 0f);
        bounds = new Bounds();
        numberOfPoints = 1;

        initialized = true;
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

    public override void SetPositions()
    {
        Initialize();
        positionBuffer.SetData(new Vector3[1]);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == detectorSelector.instantiated.gameObject)
        {
            base.OnTriggerEnter(other);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }
}
