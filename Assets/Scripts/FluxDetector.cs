using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluxDetector : FieldDetector
{
    [SerializeField]
    Material activeMaterial, inertMaterial;

    [SerializeField]
    MeshRenderer meshRenderer;

    [SerializeField]
    Mesh mesh; 

    private void Start()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        if (mesh == null)
        {
            mesh = GetComponent<MeshFilter>().mesh;
        }

        meshRenderer.material = inField ? activeMaterial : inertMaterial;
        Debug.Log("Active material? " + (inField ? true : false));
    }

    private void Update()
    {
        if(!inField)
        {
            return;
        }

        //Vector3 center = transform.position;
        //Vector3[] vertices = mesh.vertices;
        //for (int i = 0; i < vertices.Length; i ++)
        //{
        //    Vector3 normal = (vertices[i] - center).normalized; // * 0.5f + Vector3.one * 0.5f;
        //    mesh.colors[i] = new Color(normal.x, normal.y, normal.z);
        //}
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
