using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CurlLoopZone), typeof(VectorField))]
public class CurlLoopDetector : FieldDetector
{
    /// <summary>
    /// The object that sets the initial positions
    /// </summary>
    CurlLoopZone zone;

    /// <summary>
    /// The object that computes the vector field values
    /// </summary>
    VectorField localField;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        zone.fieldOrigin = detectedField.zone.fieldOrigin; // Should be attached to preCalculations
    }
}
