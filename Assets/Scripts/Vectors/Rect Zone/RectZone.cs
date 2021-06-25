using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A vector <cref>FieldZone</cref> that is a rectangular prism and has evenly-spaced points.
/// 
/// Does not support changing the dimensions in play mode or rotations in any mode. 
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class RectZone : FieldZone
{
    /// <summary>
    /// The number of points along the x direction.
    /// </summary>
    [SerializeField, Min(1)]
    int xLength = 2;
    /// <summary>
    /// The number of points along the y direction.
    /// </summary>
    [SerializeField, Min(1)]
    int yLength = 2;
    /// <summary>
    /// The number of points along the z direction.
    /// </summary>
    [SerializeField, Min(1)]
    int zLength = 2;
    [SerializeField]
    float spacing = 1;

    /// <summary>
    /// The compute shader used to calculate positions. 
    /// </summary>
    [SerializeField]
    protected ComputeShader positionCalculator;
    protected static readonly int
        positionsBufferID = Shader.PropertyToID("_Positions"),
        xLengthID = Shader.PropertyToID("_XLength"),
        yLengthID = Shader.PropertyToID("_YLength"),
        zLengthID = Shader.PropertyToID("_ZLength"),
        spacingID = Shader.PropertyToID("_Spacing"),
        matrixID = Shader.PropertyToID("_LocalToWorldMatrix");

    /// <summary>
    /// Determines whether the field parameters have been initialized. 
    /// </summary>
    protected bool initialized = false;

    /// <summary>
    /// Multiplied by spacing to calculate the <cref>maxVectorLength</cref>.
    /// </summary>
    [SerializeField]
    float vectorScalingFactor = 0.5f;





    private void Start() {
        Initialize();
    }


    // Fills the positionBuffer
    public override void SetPositions() {
        Initialize();
        // Initialize the buffer

        if(canMove) {
            CalculatePositions();
            // Calculate field origin and bounds --- dynamic
            fieldOrigin = transform.position + new Vector3(xLength - 1, yLength - 1, zLength - 1) * 0.5f * spacing;
            bounds = new Bounds(fieldOrigin, new Vector3(xLength, yLength, zLength) * spacing + 2f * Vector3.one * maxVectorLength);
        }

        //// Debugging code
        //Vector3[] positionArray = new Vector3[numberOfPoints];
        //positionBuffer.GetData(positionArray);
        //Debug.Log((("First three positions: " + positionArray[0]) + positionArray[1]) + positionArray[2]);
        //Debug.Log((("Last three positions: " + positionArray[numberOfPoints - 1]) + positionArray[numberOfPoints - 2]) +
        //    positionArray[numberOfPoints - 3]);
    }



    private void OnDisable()
    {
        if(positionBuffer != null) {
            positionBuffer.Release();
            positionBuffer = null;
        }

        initialized = false;
    }

    /// <summary>
    /// Initializes the variables that will not change until the component is disabled,
    /// but which must be created before <cref>SetPositions</cref> may be called. 
    /// </summary>
    public override void Initialize()
    {
        if(initialized) { return; }
        // Initializes the trigger collider to be certain that it is box shaped. 
        if (!(triggerCollider is BoxCollider)) {
            triggerCollider = GetComponent<BoxCollider>();
        }

        // Set some constants
        numberOfPoints = xLength * yLength * zLength;
        maxVectorLength = spacing * vectorScalingFactor;

        // Calculate field origin and bounds --- non-dynamic
        fieldOrigin = transform.position + new Vector3(xLength - 1, yLength - 1, zLength - 1) * 0.5f * spacing;
        bounds = new Bounds(fieldOrigin, new Vector3(xLength, yLength, zLength) * spacing + 2f * Vector3.one * maxVectorLength);

        // Set Collider size
        Vector3 colliderScale = new Vector3(xLength - 1, yLength - 1, zLength - 1) * spacing + 2 * Vector3.one * maxVectorLength;
        ((BoxCollider)triggerCollider).size = colliderScale;

        // Create and initialize the position buffer. 
        unsafe {
            if (positionBuffer == null || positionBuffer.count != numberOfPoints || positionBuffer.stride != sizeof(Vector3)) {
                positionBuffer = new ComputeBuffer(numberOfPoints, sizeof(Vector3));
            }
        }
        CalculatePositions();

        // Ensures that this method will not execute again until after the component has been disabled and reenabled.
        initialized = true;
    }


    protected void CalculatePositions()
    {
        // Assign the buffer to the compute shader
        positionCalculator.SetBuffer(0, positionsBufferID, positionBuffer);
        positionCalculator.SetInt(xLengthID, xLength);
        positionCalculator.SetInt(yLengthID, yLength);
        positionCalculator.SetInt(zLengthID, zLength);
        positionCalculator.SetFloat(spacingID, spacing);
        positionCalculator.SetMatrix(matrixID, transform.localToWorldMatrix);

        // Calls the compute shader
        int XGroups = Mathf.CeilToInt(xLength / 4f);
        int YGroups = Mathf.CeilToInt(yLength / 4f);
        int ZGroups = Mathf.CeilToInt(zLength / 4f);
        positionCalculator.Dispatch(0, XGroups, YGroups, ZGroups);
    }
}
