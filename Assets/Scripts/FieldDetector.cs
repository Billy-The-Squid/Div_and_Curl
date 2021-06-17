using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldDetector : MonoBehaviour
{
    protected bool inField;
    protected GPUGraph field;

    public virtual void EnteredField(GPUGraph graph)
    {
        inField = true;
        field = graph;
    }

    public virtual void ExitedField(GPUGraph graph)
    {
        inField = false;
        if(field == graph)
        {
            field = null;
        } // Better programming practice
    }
}
