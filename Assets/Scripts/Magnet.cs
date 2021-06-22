using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    /// <summary>
    /// The extra arguments provided to the Coulomb function of the vector field. 
    /// 
    /// Index 0 is the number of charges, the other indices are the strength of the charges. 
    /// </summary>
    public ComputeBuffer floatArgs;
    /// <summary>
    /// The extra vector arguments provided to the Coulomb function of the vector field. 
    /// 
    /// Index 0 is unused, the others are positions of the charges in floatArgs
    /// </summary>
    public ComputeBuffer vectorArgs;

    /// <summary>
    /// The <cref>VectorField</cref> generating the displayed field.
    /// </summary>
    [SerializeField]
    VectorField field;

    [NonSerialized]
    private float[] floatArray = { 2f, 2f, 3f };
    [NonSerialized]
    private Vector3[] vec_array = { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0) };

    void Start()
    {
        unsafe
        {
            floatArgs = new ComputeBuffer(3, sizeof(float));
            vectorArgs = new ComputeBuffer(3, sizeof(Vector3));
        }

        floatArgs.SetData(floatArray);
        vectorArgs.SetData(vec_array);

        field.preCalculations += DoThing;
    }
     
    public void DoThing()
    {
        field.floatArgsBuffer = floatArgs; // still crashes
        field.vectorArgsBuffer = vectorArgs;
        //field.floatArgsArray = floatArray;
        //field.vectorArgsArray = vec_array;
        //Array.Copy(floatArray, field.floatArgsArray, floatArray.Length);
        //Array.Copy(vec_array, field.vectorArgsArray, vec_array.Length);
    }

    // Make sure to wipe the Compute buffers after use. Otherwise, the GPU will complain!
    private void OnDestroy()
    {
        floatArgs.Release();
        floatArgs = null;

        vectorArgs.Release();
        vectorArgs = null;
    }
}
