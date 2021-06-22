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

    // The arrays used to initialize the argument buffers. 
    [NonSerialized]
    private float[] floatArray = { 2f, -2f, 3f };
    [NonSerialized]
    private Vector3[] vec_array = { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0) };
    // Eventually, these will be set based on a position



    void Start()
    {
        // Create the ComputeBuffers
        unsafe {
            floatArgs = new ComputeBuffer(3, sizeof(float));
            vectorArgs = new ComputeBuffer(3, sizeof(Vector3));
        }

        // Initialize the ComputeBuffers
        floatArgs.SetData(floatArray);
        vectorArgs.SetData(vec_array);

        // VectorFields will call SetExtraArgs before every calculation.
        field.preCalculations += SetExtraArgs;
    }
    
    /// <summary>
    /// Sets the extra float and vector arguments of the <cref>VectorField</cref>
    /// </summary>
    public void SetExtraArgs()
    {
        field.floatArgsBuffer = floatArgs; 
        field.vectorArgsBuffer = vectorArgs;
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
