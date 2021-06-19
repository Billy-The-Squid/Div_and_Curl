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



    public override void SetPositions()
    {
        // Initialize the buffer
        int numberOfPoints = xLength * yLength * zLength;
        unsafe {
            if (positionBuffer == null || positionBuffer.count != numberOfPoints || positionBuffer.stride != sizeof(Vector3)) {
                positionBuffer = new ComputeBuffer(numberOfPoints, sizeof(Vector3));
            }
        }

        // Assign the buffer to the compute shader
        positionCalculator.SetBuffer(0, "_Positions", positionBuffer);
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

        // Calculate field origin and bounds
        fieldOrigin = transform.position + new Vector3(xLength - 1, yLength - 1, zLength - 1) * 0.5f * spacing;
        bounds = new Bounds(fieldOrigin, 2 * fieldOrigin - transform.position + Vector3.one * maxVectorLength);

        // Set Collider size
        Vector3 colliderScale = new Vector3(xLength, yLength, zLength) * spacing + 2 * Vector3.one * maxVectorLength;
        triggerCollider.transform.localScale = colliderScale;
    }



    private void OnDisable()
    {
        if(positionBuffer != null)
        {
            positionBuffer.Release();
            positionBuffer = null;
        }
    }
}
