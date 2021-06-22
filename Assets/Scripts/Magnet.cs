using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    public ComputeBuffer floatArgs, vectorArgs;

    [SerializeField]
    VectorField field;

    [NonSerialized]
    public float[] floatArray = { 2f, 2f, 3f };
    [NonSerialized]
    public Vector3[] vec_array = { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0) };

    void Start()
    {
        unsafe
        {
            floatArgs = new ComputeBuffer(5, sizeof(float));
            vectorArgs = new ComputeBuffer(3, sizeof(Vector3));
        }

        floatArgs.SetData(floatArray);
        vectorArgs.SetData(vec_array);

        field.preCalculations += DoThing;
    }
     
    public void DoThing()
    {
        //field.computeShader.SetBuffer(2, "_FloatArgs", floatArgs);
        //field.computeShader.SetBuffer(2, "_VectorArgs", vectorArgs);
        //field.floatArgsBuffer = floatArgs; // still crashes
        //field.vectorArgsBuffer = vectorArgs;
        //Array.Copy(floatArray, field.floatArgsArray, 3);
        //Debug.Log("Trying to do thing");
        field.floatArgsArray = floatArray;
        //Debug.Log("Did thing");
        field.vectorArgsArray = vec_array;
    }

    private void OnDestroy()
    {
        floatArgs.Release();
        floatArgs = null;

        vectorArgs.Release();
        vectorArgs = null;
    }
}
