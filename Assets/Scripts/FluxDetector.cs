using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluxDetector : FieldDetector
{
    [SerializeField]
    Material activeMaterial, inertMaterial;

    [SerializeField]
    MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer.material = inField ? activeMaterial : inertMaterial;
        Debug.Log("Active material? " + (inField ? true : false));
    }

    private void Update()
    {
        if(!inField)
        {
            return;
        }

        // Do something with the shader. 
    }





    public override void EnteredField(GPUGraph graph)
    {
        meshRenderer.material = activeMaterial;
        base.EnteredField(graph);
    }

    public override void ExitedField(GPUGraph graph)
    {
        meshRenderer.material = inertMaterial;
        base.ExitedField(graph);
    }
}
