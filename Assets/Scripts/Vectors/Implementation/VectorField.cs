using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// This class is built to manage the interactions between the different scripts that create a vector field. 
/// Given a FieldZone, it calls upon that for the positions of each vector to be plotted, then uses that 
/// to calculate the value of the buffer at each point. Finally, it plots these results with GPU instancing.
/// 
/// Does not currently support a changing number of points. 
/// </summary>
[RequireComponent(typeof(FieldZone), typeof(Display))]
public class VectorField : MonoBehaviour
{
    /// <summary>
    /// The <cref>FieldZone</cref> object used to determine positions.
    /// </summary>
    public FieldZone zone;

    /// <summary>
    /// The buffer in which the vector worldspace positions are stored.
    /// </summary>
    public ComputeBuffer positionsBuffer { get; protected set; }
    /// <summary>
    /// The buffer in which the vector values are stored. 
    /// Same indexing scheme as <cref>positionsBuffer</cref>.
    /// </summary>
    public ComputeBuffer vectorsBuffer { get; protected set; }
    /// <summary>
    /// Stores the extra float arguments used in the computation.
    /// Set your own indexing scheme and initialize by subscribing
    /// to the `preCalculations` delegate.
    /// </summary>
    public ComputeBuffer floatArgsBuffer { get; set; }
    /// <summary>
    /// Stores the extra vector arguments used in the computation.
    /// Set your own indexing scheme and initialize by subscribing
    /// to the `preCalculations` delegate.
    /// </summary>
    public ComputeBuffer vectorArgsBuffer { get; set; }
    

    /// <summary>
    /// The number of points at which vectors will be plotted and the number of values in each buffer.
    /// </summary>
    int numOfPoints;

    /// <summary>
    /// The compute shader used to generate the vector field. 
    /// </summary>
    public ComputeShader computeShader;

    // Property IDs used to send values to various shaders.
    static readonly int
        centerID = Shader.PropertyToID("_CenterPosition"),
        positionsBufferID = Shader.PropertyToID("_Positions"),
        vectorBufferID = Shader.PropertyToID("_Vectors"),
        floatArgsID = Shader.PropertyToID("_FloatArgs"),
        vectorArgsID = Shader.PropertyToID("_VectorArgs");


    /// <summary>
    /// The possible types of field to display. 
    /// It is the user's responsibility to make sure that these selections align with those in FieldLibrary.hlsl
    /// </summary>
    public enum FieldType { Outwards, Swirl, Coulomb, Vortices, Galaxy, PlaneWave, Inwards, Squish, Empty }
    /// <summary>
    /// The type of field to be displayed. Cannot be changed in Play Mode if <cref>isDynamic</cref> is set to False.
    /// </summary>
    [SerializeField]
    protected FieldType _fieldType;
    public FieldType fieldType
    {
        get => _fieldType;
        set
        {
            if(_fieldType != value)
            {
                StartCoroutine(RefreshField());
                _fieldType = value;
            }
        }
    }


    /// <summary>
    /// Set this to true if the field values should be updated each frame. 
    /// Requires more GPU time. 
    /// 
    /// Tip: this can be toggled on during play mode to force the field to recalculate, then 
    /// toggled back off. 
    /// </summary>
    [SerializeField]
    public bool isDynamic;
    /// <summary>
    /// Indicates whether the vectors buffer has been initialized. For non-dynamic fields. 
    /// </summary>
    private bool hasBeenCalculated;

    public Display display { get; protected set; }



    public delegate void Reminder();
    /// <summary>
    /// This delegate will get called prior to setting the positions buffer.
    /// </summary>
    public Reminder preSetPositions;
    /// <summary>
    /// This delegate will get called prior to setting the vectors buffer. 
    /// </summary>
    public Reminder preCalculations;
    /// <summary>
    /// This delegate will get called prior to displaying the field. 
    /// </summary>
    public Reminder preDisplay;

    [Tooltip("Called after a script changes the field type.")]
    public UnityEvent OnFieldChange = new UnityEvent();





    private void Awake() {
        if (zone == null) {
            zone = GetComponent<FieldZone>();
        }
        if (display == null) {
            display = GetComponent<Display>();
        }
    }

    private void OnEnable()
    {
        if (preSetPositions != null) { preSetPositions(); }

        zone.SetPositions();

        positionsBuffer = zone.positionBuffer;
        numOfPoints = positionsBuffer.count;

        unsafe // <-- This could maybe be a source of problems.
        {
            vectorsBuffer = new ComputeBuffer(numOfPoints, sizeof(Vector3)); // last arg: size of single object
        }
    }



    private void OnDisable()
    {
        if(vectorsBuffer != null)
        {
            vectorsBuffer.Release();
            vectorsBuffer = null;
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (preSetPositions != null) { preSetPositions(); }
        zone.SetPositions();

        if (zone.canMove) {
            isDynamic = true;
        }

        
        if(isDynamic || !hasBeenCalculated)
        {
            if (preCalculations != null) { preCalculations(); }
            CalculateVectors();
            hasBeenCalculated = true;

            display.maxVectorLength = zone.maxVectorLength;
            display.bounds = zone.bounds;
        }

        {
            //// Debug code
            //Vector3[] debugArray = new Vector3[numOfPoints];
            //vectorsBuffer.GetData(debugArray);
            //Debug.Log((("First three points in vector array: " + debugArray[0]) + debugArray[1]) + debugArray[2]);
            //Debug.Log((("Last three points in vector array: " + debugArray[numOfPoints - 1]) + debugArray[numOfPoints - 2]) + debugArray[numOfPoints - 3]);
        }

        //if(fieldType == FieldType.Empty) // Helpful someday, but annoying right now.
        //{
        //    Debug.LogWarning("Using empty field");
        //}
    }

    private void LateUpdate() // WHAT REQUIRES THIS? %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    {
        if (preDisplay != null) { preDisplay(); }
        display.DisplayVectors(positionsBuffer, vectorsBuffer);
    }

    public IEnumerator RefreshField () {
        bool currentlyDynamic = isDynamic;
        isDynamic = true;
        yield return new WaitForSeconds(0.1f);

        isDynamic = currentlyDynamic;
        if(OnFieldChange != null)
        {
            OnFieldChange.Invoke();
        }
    }

    /// <summary>
    /// Interfaces with the <cref>computeShader</cref> and calculates the value of the vectors at each point, storing them in the buffers. 
    /// </summary>
    private void CalculateVectors()
    {
        // The data is sent to the computeShader for calculation
        computeShader.SetVector(centerID, zone.fieldOrigin);

        int kernelID = (int)fieldType;
        computeShader.SetBuffer(kernelID, positionsBufferID, positionsBuffer);
        computeShader.SetBuffer(kernelID, vectorBufferID, vectorsBuffer);
        if(floatArgsBuffer != null) {
            computeShader.SetBuffer(kernelID, floatArgsID, floatArgsBuffer);
        }
        if(vectorArgsBuffer != null) {
            computeShader.SetBuffer(kernelID, vectorArgsID, vectorArgsBuffer);
        }

        // This does the math and stores information in the positionsBuffer.
        int groups = Mathf.CeilToInt(numOfPoints / 64f);
        computeShader.Dispatch(kernelID, groups, 1, 1);
    }

    public void Pass()
    {
        ;
    }

    private void OnDrawGizmos()
    {
        if(display != null && display.bounds != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(display.bounds.center, display.bounds.size);
        }
    }
}
