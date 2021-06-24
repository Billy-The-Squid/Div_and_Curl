using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainyRectZone : FieldZone
{
    [SerializeField, Min(1f)]
    protected float xLength, yLength, zLength;

    [SerializeField, Min(0.0f)]
    protected float lifespan;

    [SerializeField]
    int pointsCount;

    public ComputeShader positionCalculator;
    public ComputeBuffer timesBuffer;
    protected static readonly int
        positionsBufferID = Shader.PropertyToID("_Positions"),
        timesBufferID = Shader.PropertyToID("_Times"),
        xLengthID = Shader.PropertyToID("_XLength"),
        yLengthID = Shader.PropertyToID("_YLength"),
        zLengthID = Shader.PropertyToID("_ZLength"),
        numPointsID = Shader.PropertyToID("_NumberOfPoints"),
        lifespanID = Shader.PropertyToID("_Lifespan"),
        timeID = Shader.PropertyToID("_Time"),
        matrixID = Shader.PropertyToID("_LocalToWorldMatrix");

    protected bool initialized = false;

    float vectorScalingFactor = 0.5f;



    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    private void OnDisable()
    {
        if (positionBuffer != null)
        {
            positionBuffer.Release();
            positionBuffer = null;
        }
        if (timesBuffer != null)
        {
            timesBuffer.Release();
            timesBuffer = null;
        }

        initialized = false;
    }



    public override void Initialize()
    {
        if(initialized) { return; }

        canMove = true;

        if (!(triggerCollider is BoxCollider))
        {
            triggerCollider = GetComponent<BoxCollider>();
        }

        // Set some constants
        numberOfPoints = pointsCount;
        maxVectorLength = vectorScalingFactor;

        // Calculate field origin and bounds --- non-dynamic
        fieldOrigin = transform.position + new Vector3(xLength, yLength, zLength) * 0.5f;
        bounds = new Bounds(fieldOrigin, new Vector3(xLength, yLength, zLength) + 2f * Vector3.one * maxVectorLength);

        // Set Collider size
        Vector3 colliderScale = new Vector3(xLength, yLength, zLength) + 2 * maxVectorLength * Vector3.one;
        ((BoxCollider)triggerCollider).size = colliderScale;

        // Create and initialize the position buffer. 
        unsafe
        {
            if (positionBuffer == null || positionBuffer.count != numberOfPoints || positionBuffer.stride != sizeof(Vector3))
            {
                positionBuffer = new ComputeBuffer(numberOfPoints, sizeof(Vector3));
            }
            if (timesBuffer == null)
            {
                timesBuffer = new ComputeBuffer(numberOfPoints, sizeof(float));
            }
            DebugDisplay();
        }
        CalculatePositions(1);

        DebugDisplay();

        initialized = true;
    }

    public override void SetPositions() {
        Initialize();
        CalculatePositions(0);

        //DebugDisplay();
    }

    private void CalculatePositions(int kernelID)
    {
        // Assign the buffer to the compute shader
        positionCalculator.SetBuffer(kernelID, positionsBufferID, positionBuffer);
        positionCalculator.SetBuffer(kernelID, timesBufferID, timesBuffer);
        positionCalculator.SetInt(numPointsID, numberOfPoints);
        positionCalculator.SetFloat(xLengthID, xLength);
        positionCalculator.SetFloat(yLengthID, yLength);
        positionCalculator.SetFloat(zLengthID, zLength);
        positionCalculator.SetFloat(lifespanID, lifespan);
        positionCalculator.SetFloat(timeID, Time.time);
        positionCalculator.SetMatrix(matrixID, transform.localToWorldMatrix);

        // Calls the compute shader
        int numGroups = Mathf.CeilToInt(pointsCount / 64f);
        positionCalculator.Dispatch(kernelID, numGroups, 1, 1);
    }

    void DebugDisplay()
    {
        //// Debugging code
        Vector3[] positionArray = new Vector3[numberOfPoints];
        positionBuffer.GetData(positionArray);
        Debug.Log((("First three positions: " + positionArray[0]) + positionArray[1]) + positionArray[2]);
        Debug.Log((("Last three positions: " + positionArray[numberOfPoints - 1]) + positionArray[numberOfPoints - 2]) +
            positionArray[numberOfPoints - 3]);
        float[] timesArray = new float[numberOfPoints];
        timesBuffer.GetData(timesArray);
        Debug.Log((("First three times: " + timesArray[0] + ", ") + timesArray[1] + ", ") + timesArray[2]);
        Debug.Log((("Last three times: " + timesArray[numberOfPoints - 1] + ", ") + timesArray[numberOfPoints - 2] + ", ") +
            timesArray[numberOfPoints - 3]);
        Debug.Log("Time: " + Time.time);
    }
}
