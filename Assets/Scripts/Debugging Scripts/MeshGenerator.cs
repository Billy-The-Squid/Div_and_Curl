using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshGenerator : MonoBehaviour
{
    //public Mesh[] meshes;

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        AssetDatabase.CreateAsset(mesh, "Assets/" + mesh.name + ".asset");

        //int i = 0;
        //foreach (Mesh mesh in meshes)
        //{
        //    AssetDatabase.CreateAsset(mesh, "Assets/" + mesh.name + (i + ".mesh"));
        //    i++;
        //}
        AssetDatabase.SaveAssets();
    }
}
