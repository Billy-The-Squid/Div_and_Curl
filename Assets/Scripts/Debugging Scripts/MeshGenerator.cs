using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        AssetDatabase.CreateAsset(mesh, "Assets/" + mesh.name +".asset");
        AssetDatabase.SaveAssets();
    }
}
