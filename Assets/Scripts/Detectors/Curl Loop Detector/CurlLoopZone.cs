using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurlLoopZone : FieldZone
{
    /// <summary>
    /// The number of points to take the integral at.
    /// </summary>
    [SerializeField, Min(4)]
    public int resolution = 20;

    /// <summary>
    /// Has the field been initialized?
    /// </summary>
    protected bool initialized = false;

    /// <summary>
    /// The array storing the positions. 
    /// </summary>
    Vector3[] posArray;

    /// <summary>
    /// Stores the tangent vectors going around the loop. 
    /// </summary>
    public ComputeBuffer tangentBuffer { get; protected set; }

    /// <summary>
    /// Array storing the tangent vectors
    /// </summary>
    Vector3[] tanArray;

    [Tooltip("The scaling factors for vector visualizations")]
    public float scalingFactor = 2;







    // Start is called before the first frame update
    void OnEnable() {
        Initialize();
    }

    private void OnDisable()
    {
        initialized = false;

        if(positionBuffer != null)
        {
            positionBuffer.Release();
            positionBuffer = null;
        }
        if(tangentBuffer != null)
        {
            tangentBuffer.Release();
            tangentBuffer = null;
        }
    }





    public override void Initialize()
    {
        if(initialized) { return; }

        // Set the positions
        unsafe {
            positionBuffer = new ComputeBuffer(resolution, sizeof(Vector3));
            tangentBuffer = new ComputeBuffer(resolution, sizeof(Vector3));
        }
        posArray = new Vector3[resolution];
        tanArray = new Vector3[resolution];

        // Other important variables
        numberOfPoints = resolution;
        canMove = true;

        initialized = true;
    }



    public override void SetPositions()
    {
        Initialize();

        bounds = new Bounds(transform.position, transform.localScale * 2 + Vector3.one * maxVectorLength * 2);
        maxVectorLength = 2 * Mathf.PI * transform.localScale.x * scalingFactor / resolution; // Not the best programming practice...

        for (int i = 0; i < resolution; i++)
        {
            posArray[i] = new Vector3(Mathf.Cos(2 * Mathf.PI * i / resolution), Mathf.Sin(2 * Mathf.PI * i / resolution), 0f);
            posArray[i] = transform.TransformPoint(posArray[i]);

            tanArray[i] = new Vector3(-1 * Mathf.Sin(2 * Mathf.PI * i / resolution), Mathf.Cos(2 * Mathf.PI * i / resolution), 0f) 
                * 2 * Mathf.PI / resolution; // won't work on other shapes. 
            tanArray[i] = transform.TransformVector(tanArray[i]);
        }
        positionBuffer.SetData(posArray);
        tangentBuffer.SetData(tanArray);

        //// Debug code
        //debugDisplay.bounds = bounds;
        //debugDisplay.DisplayVectors(positionBuffer, tangentBuffer);
    }
}
