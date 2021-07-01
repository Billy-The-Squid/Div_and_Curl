using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurlSpinZone : FieldZone
{
    /// <summary>
    /// Checks whether we've initialized things.
    /// </summary>
    protected bool initialized = false;

    /// <summary>
    /// An array holding the current positions of each of the calculation points
    /// </summary>
    Vector3[] positionsArray;

    /// <summary>
    /// An array holding the localspace offset of each calculation point. 
    /// </summary>
    Vector3[] localPositionsArray;

    /// <summary>
    /// The distance used in the derivative computation.
    /// </summary>
    public float deltaX = 0.01f;




    private void OnDisable()
    {
        if(positionBuffer != null)
        {
            positionBuffer.Release();
            positionBuffer = null;
        }
    }

    public override void Initialize()
    {
        if(initialized) { return; }

        // Create the array of local-position computation points
        localPositionsArray = new Vector3[7];
        localPositionsArray[0] = Vector3.zero;
        localPositionsArray[1] = Vector3.right * deltaX; // +x 
        localPositionsArray[2] = Vector3.left * deltaX;
        localPositionsArray[3] = Vector3.forward * deltaX; // +z in Unity units, +y in real-world
        localPositionsArray[4] = Vector3.back * deltaX;
        localPositionsArray[5] = Vector3.up * deltaX; // +y in Unity units, +z in real-world. 
        localPositionsArray[6] = Vector3.down * deltaX;

        positionsArray = new Vector3[7];

        // Create the positions buffer
        unsafe
        {
            positionBuffer = new ComputeBuffer(7, sizeof(Vector3));
        }

        canMove = true;
        initialized = true;
    }

    public override void SetPositions()
    {
        Initialize();

        // Set the position buffer
        for(int i = 0; i < 7; i++)
        {
            positionsArray[i] = localPositionsArray[i] + transform.position;
        }
        positionBuffer.SetData(positionsArray);
    }
}