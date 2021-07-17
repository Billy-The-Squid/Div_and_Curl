using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainyDisplay : VectorDisplay
{
    protected ComputeBuffer timesBuffer;
    [SerializeField]
    RainyRectZone zone;

    protected float lifespan;

    



    private void Start()
    {
        timesBuffer = zone.timesBuffer;
        lifespan = zone.lifespan;
    }

    protected override void PlotResults(ComputeBuffer positionsBuffer)
    {
        pointerMaterial.SetBuffer("_Times", timesBuffer);
        pointerMaterial.SetFloat("_Lifespan", lifespan);
        base.PlotResults(positionsBuffer);
    }


    private void OnDestroy()
    {
        if (PlotVectorsBuffer != null)
        {
            PlotVectorsBuffer.Release();
            PlotVectorsBuffer = null;
        }
        if (Vector2Buffer != null)
        {
            Vector2Buffer.Release();
            Vector2Buffer = null;
        }
        if (Vector3Buffer != null)
        {
            Vector3Buffer.Release();
            Vector3Buffer = null;
        }
        if (MagnitudesBuffer != null)
        {
            MagnitudesBuffer.Release();
            MagnitudesBuffer = null;
        }
        //if (maxMagnitude != null)
        //{
        //    maxMagnitude.Release();
        //    maxMagnitude = null;
        //}

        Initialized = false;
    }
}
