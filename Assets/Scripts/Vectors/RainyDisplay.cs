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
        if (plotVectorsBuffer != null)
        {
            plotVectorsBuffer.Release();
            plotVectorsBuffer = null;
        }
        if (vector2Buffer != null)
        {
            vector2Buffer.Release();
            vector2Buffer = null;
        }
        if (vector3Buffer != null)
        {
            vector3Buffer.Release();
            vector3Buffer = null;
        }
        if (magnitudesBuffer != null)
        {
            magnitudesBuffer.Release();
            magnitudesBuffer = null;
        }
        if (maxMagnitude != null)
        {
            maxMagnitude.Release();
            maxMagnitude = null;
        }

        initialized = false;
    }
}
