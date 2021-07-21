using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VectorField))]
public class PointZone : FieldZone
{
    /// <summary>
    /// The transition array holding the transform position. 
    /// </summary>
    protected Vector3[] posArray = new Vector3[1];

    /// <summary>
    /// The vector field using this zone. 
    /// </summary>
    public VectorField localField;

    [Tooltip("The factor by which the vector's length is multiplied before displaying.")]
    public float scalingFactor = 1f;

    /// <summary>
    /// Keeps track of whether the zone has been initialized. 
    /// </summary>
    protected bool initialized = false;





    // Start is called before the first frame update
    void Start()
    {
        if(localField == null) {
            localField = GetComponent<VectorField>();
        }
    }

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

        // Initialize the position buffer
        unsafe {
            positionBuffer = new ComputeBuffer(1, sizeof(Vector3));
        }

        // Set some important variables.
        canMove = true;
        maxVectorLength = scalingFactor * transform.localScale.x; // Assuming uniform scaling in each direction.

        initialized = true;
    }

    public override void SetPositions()
    {
        Initialize();

        posArray[0] = transform.position;
        positionBuffer.SetData(posArray);

        bounds = new Bounds(transform.position, new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z));
        numberOfPoints = 1; 
    }
}
