using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RectZone : FieldZone
{
    [SerializeField, Min(1)]
    int xLength = 2, yLength = 2, zLength = 2;
    [SerializeField]
    float spacing = 1;

    [SerializeField]
    private ComputeShader positionCalculator;
    private static readonly int
        positionsBufferID = Shader.PropertyToID("_Positions"),
        xLengthID = Shader.PropertyToID("_XLength"),
        yLengthID = Shader.PropertyToID("_YLength"),
        zLengthID = Shader.PropertyToID("_ZLength"),
        spacingID = Shader.PropertyToID("_Spacing"),
        originID = Shader.PropertyToID("_OriginPosition");

    private bool initialized = false;





    private void Start()
    {
        Initialize();
    }

    public override void SetPositions()
    {
        Initialize();
        // Initialize the buffer
        unsafe {
            if (positionBuffer == null || positionBuffer.count != numberOfPoints || positionBuffer.stride != sizeof(Vector3)) {
                positionBuffer = new ComputeBuffer(numberOfPoints, sizeof(Vector3));
            }
        }

        // Assign the buffer to the compute shader
        positionCalculator.SetBuffer(0, positionsBufferID, positionBuffer);
        positionCalculator.SetInt(xLengthID, xLength);
        positionCalculator.SetInt(yLengthID, yLength);
        positionCalculator.SetInt(zLengthID, zLength);
        positionCalculator.SetFloat(spacingID, spacing);
        positionCalculator.SetVector(originID, transform.position); // Not to be confused with fieldOrigin...

        // Calls the compute shader
        int XGroups = Mathf.CeilToInt(xLength / 4f);
        int YGroups = Mathf.CeilToInt(yLength / 4f);
        int ZGroups = Mathf.CeilToInt(zLength / 4f);
        positionCalculator.Dispatch(0, XGroups, YGroups, ZGroups);

        //// Debugging code
        ////Debug.Log("Tranform position: " + transform.position);
        //Vector3[] positionArray = new Vector3[numberOfPoints];
        ////Debug.Log("Buffer length: " + positionBuffer.count);
        //positionBuffer.GetData(positionArray);
        //Debug.Log((("First three positions: " + positionArray[0]) + positionArray[1]) + positionArray[2]);
        //Debug.Log((("Last three positions: " + positionArray[numberOfPoints - 1]) + positionArray[numberOfPoints - 2]) +
        //    positionArray[numberOfPoints - 3]);
    }



    private void OnDisable()
    {
        if(positionBuffer != null)
        {
            positionBuffer.Release();
            positionBuffer = null;
        }

        initialized = false;
    }

    public override void Initialize()
    {
        if(initialized) { return; }
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<BoxCollider>();
        }
        triggerCollider.isTrigger = true;

        numberOfPoints = xLength * yLength * zLength;

        // Calculate field origin and bounds
        fieldOrigin = transform.position + new Vector3(xLength - 1, yLength - 1, zLength - 1) * 0.5f * spacing;
        bounds = new Bounds(fieldOrigin, 2 * fieldOrigin - transform.position + Vector3.one * maxVectorLength);

        // Set Collider size
        Vector3 colliderScale = new Vector3(xLength - 1, yLength - 1, zLength - 1) * spacing + 2 * Vector3.one * maxVectorLength;
        ((BoxCollider)triggerCollider).size = colliderScale;

        initialized = true;
    }
}
