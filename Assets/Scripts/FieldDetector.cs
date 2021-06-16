using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldDetector : MonoBehaviour
{
    protected bool inField;

    public virtual void EnteredField(GPUGraph graph)
    {
        inField = true;
    }

    public virtual void ExitedField(GPUGraph graph)
    {
        inField = false;
    }
}
