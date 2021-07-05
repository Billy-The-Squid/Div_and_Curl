using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TerrainGenerator : MonoBehaviour
{
    Mesh mesh;
    [SerializeField]
    MeshCollider meshCollider;
    [SerializeField]
    MeshFilter meshFilter;
    [SerializeField]
    MeshRenderer meshRenderer;

    public int size;

    Vector3[] vertices;
    int[] triangles;

    private delegate float Function(float x, float y); // Maybe change to take floats?

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        //vertices = new Vector3[size * size];
        vertices = new Vector3[2 * 3 * (size - 1) * (size - 1)];
        triangles = new int[2 * 3 * (size - 1) * (size - 1)];


        GenerateMesh(mesh);
        meshFilter.mesh = mesh;

        for(int i = 0; i < 10; i++)
        {
            Debug.Log("Perlin noise " + i + ": " + 10 * (Mathf.PerlinNoise(((float) i) / 10, 0.0f) - 0.5f));
        }

        //AssetDatabase.CreateAsset(mesh, "Assets/terrain_3.asset");
        //AssetDatabase.SaveAssets();
    }



    //private void GenerateMesh(Mesh mesh)
    //{
    //    Function function = quadratic;

    //    // Generate the vertices
    //    for(int i = 0; i < size; i++)
    //    {
    //        for(int j = 0; j < size; j++)
    //        {
    //            int index = GetIndex(i, j);

    //            vertices[index] = new Vector3(i, function(i, j), j);
    //        }
    //    }
    //    //Debug.Log("Vertices: " + vertices);
    //    mesh.vertices = vertices;

    //    // Making the triangles
    //    int triIndex = 0;
    //    for(int i = 0; i < size - 1; i++)
    //    {
    //        for(int j = 0; j < size - 1; j++)
    //        {
    //            triangles[triIndex++] = GetIndex(i, j);
    //            triangles[triIndex++] = GetIndex(i, j+1);
    //            triangles[triIndex++] = GetIndex(i+1, j);

    //            triangles[triIndex++] = GetIndex(size - i - 1, size - j - 1);
    //            triangles[triIndex++] = GetIndex(size - i - 1, size - j - 2);
    //            triangles[triIndex++] = GetIndex(size - i - 2, size - j - 1);
    //        }
    //    }
    //    //Debug.Log("Triangles: " + triangles);
    //    mesh.triangles = triangles;

    //    mesh.RecalculateNormals();
    //}

    private void GenerateMesh(Mesh mesh)
    {
        Function function = terrain_3;

        // Generate the non-redundant vertices
        Vector3[] NRVertices = new Vector3[size * size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int index = GetIndex(i, j);

                NRVertices[index] = new Vector3(i, function(i, j), j);
            }
        }
        //Debug.Log("Vertices: " + vertices);

        // Making the triangles
        int triIndex = 0;
        for (int i = 0; i < size - 1; i++)
        {
            for (int j = 0; j < size - 1; j++)
            {
                triangles[triIndex++] = GetIndex(i, j);
                triangles[triIndex++] = GetIndex(i, j + 1);
                triangles[triIndex++] = GetIndex(i + 1, j);

                triangles[triIndex++] = GetIndex(size - i - 1, size - j - 1);
                triangles[triIndex++] = GetIndex(size - i - 1, size - j - 2);
                triangles[triIndex++] = GetIndex(size - i - 2, size - j - 1);
            }
        }
        //Debug.Log("Triangles: " + triangles);

        Vector3 cursor;
        for(int i = 0; i < triangles.Length; i++)
        {
            cursor = NRVertices[triangles[i]];
            vertices[i] = new Vector3(cursor.x, cursor.y, cursor.z);
            triangles[i] = i;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    private int GetIndex(int x, int y)
    {
        return x + size * y;
    }

    private float quadratic(int x, int y)
    {
        return x * x + y * y;
    }

    private float sine(int x, int y)
    {
        return 2 * Mathf.Sin(Mathf.Sqrt(x * x + y * y));
    }

    private float terrain(float x, float y)
    {
        float val = 10 * (Mathf.PerlinNoise(((float)x) / 10, y / 10) - 0.5f);
        float cutoff = -2;
        if (val < cutoff)
        {
            val = cutoff;
        }
        return val;
    }

    private float terrain_1(float x, float y)
    {
        float val = 10 * (Mathf.PerlinNoise(x / 20, (26-(y)) / 20) - 0.5f);
        float cutoff = -2;
        if (val < cutoff)
        {
            val = cutoff;
        }
        return (val - cutoff) * 2;
    }

    private float terrain_2(float x, float y)
    {
        float val = 9 * (Mathf.PerlinNoise(x / 20, (26 - (y)) / 20) - 0.5f) + 1 * (Mathf.PerlinNoise(x / 5, y / 5) - 0.5f);
        float cutoff = -2;
        if (val < cutoff)
        {
            val = cutoff;
        }
        return (val - cutoff) * 2;
    }

    private float terrain_3(float x, float y)
    {
        float val = 9 * (Mathf.PerlinNoise(x / 20, (26 - (y)) / 20) - 0.5f) + 1 * (Mathf.PerlinNoise(x / 5, y / 5) - 0.5f) - 0.1f * (x-15);
        float cutoff = -1;
        if (val < cutoff)
        {
            val = cutoff;
        }
        return (val - cutoff) * 2;
    }
}
