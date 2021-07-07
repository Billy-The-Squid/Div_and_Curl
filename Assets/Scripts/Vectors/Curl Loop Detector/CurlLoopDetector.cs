using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CurlLoopZone), typeof(VectorField))]
public class CurlLoopDetector : FieldDetector
{
    /// <summary>
    /// The object that sets the initial positions
    /// </summary>
    [SerializeField]
    CurlLoopZone zone;

    /// <summary>
    /// The object that computes the vector field values
    /// </summary>
    [SerializeField]
    VectorField localField;

    /// <summary>
    /// The compute shader used to compute the integral
    /// </summary>
    [SerializeField]
    ComputeShader integrator;

    /// <summary>
    /// The buffer that stores the results.
    /// </summary>
    [SerializeField]
    ComputeBuffer curlBuffer;








    // Start is called before the first frame update
    void Start()
    {
        // Setting some variables
        if(zone == null) {
            zone = GetComponent<CurlLoopZone>();
        }
        if(localField == null)
        {
            localField = GetComponent<VectorField>();
        }

        //localField.preCalculations += 
    }

    // Update is called once per frame
    void Update()
    {
        zone.fieldOrigin = detectedField.zone.fieldOrigin; // Should be attached to preCalculations


    }
}
