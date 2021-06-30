using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VectorField), typeof(Rigidbody), typeof(CurlSpinZone))]
public class CurlSpinDetector : FieldDetector
{
    /// <summary>
    /// The vector curl at the center of the detector.
    /// </summary>
    Vector3 curl;

    /// <summary>
    /// The compute shader used to calculate the curl.
    /// </summary>
    [SerializeField]
    ComputeShader curlComputer;

    /// <summary>
    /// The <cref>VectorField</cref> that directs the field computation
    /// </summary>
    VectorField computationField;

    /// <summary>
    /// The detector's RigidBody.
    /// </summary>
    Rigidbody rigidBody;






    // Start is called before the first frame update
    void Start()
    {
        if(!inField)
        {
            computationField.enabled = false; // I hope this is safe...
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!inField) { return; } // What should this detector do when it isn't in a field?

        // These should be attached to preCalculation
        computationField.fieldType = detectedField.fieldType;
        computationField.zone.fieldOrigin = detectedField.zone.fieldOrigin;

        // This should be attached to preDisplay
        CalculateCurl();

        rigidBody.angularVelocity = curl; // Scale this so that the visual rate of spin matches the rate that particles will move
    }



    /// <summary>
    /// Calculates the local curl and assigns that to <cref>curl</cref>
    /// </summary>
    private void CalculateCurl()
    {
        

        throw new NotImplementedException();
    }



    public override void EnteredField(VectorField graph)
    {
        computationField.enabled = true;
        base.EnteredField(graph);
    }

    public override void ExitedField(VectorField graph)
    {
        computationField.enabled = false;
        base.ExitedField(graph);
        rigidBody.angularVelocity = Vector3.zero;
    }
}
