using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VectorDisplay : Display
{
    /// <summary>
    /// The buffer in which the visual magnitudes of each vector are stored. 
    /// Same indexing scheme as <cref>positionsBuffer</cref>.
    /// </summary>
    public ComputeBuffer plotVectorsBuffer { get; protected set; }
    /// <summary>
    /// One of two buffers in which values used for calculating the transformation matrix for vectors are stored.
    /// Same indexing scheme as <cref>positionsBuffer</cref>.
    /// 
    /// Contains vectors orthogonal to those in <cref>plotVectorsBuffer</cref>, with the same magnitude, in order 
    /// to generate an orthogonal basis. 
    /// </summary>
    public ComputeBuffer vector2Buffer { get; protected set; }
    /// <summary>
    /// One of two buffers in which values used for calculating the transformation matrix for vectors are stored.
    /// Same indexing scheme as <cref>positionsBuffer</cref>.
    /// 
    /// Contains vectors orthogonal to those in <cref>plotVectorsBuffer</cref>, with the same magnitude, in order 
    /// to generate an orthogonal basis. 
    /// </summary>
    public ComputeBuffer vector3Buffer { get; protected set; }


    /// <summary>
    /// Stores the magnitudes of the vectors in <cref>vectorsBuffer</cref>. 
    /// Same indexing scheme as <cref>positionsBuffer</cref>.
    /// </summary>
    public ComputeBuffer magnitudesBuffer { get; protected set; }
    ///// <summary>
    ///// 
    ///// </summary>
    //protected ComputeBuffer maxMagnitude;
    //float[] maxMagnitudeArray;

    /// <summary>
    /// The number of points at which vectors will be plotted and the number of values in each buffer.
    /// </summary>
    int numOfPoints;

    static readonly int
        numPointsID = Shader.PropertyToID("_NumberOfPoints"),
        positionsBufferID = Shader.PropertyToID("_Positions"),
        vectorsBufferID = Shader.PropertyToID("_Vectors"),
        plotVectorsBufferID = Shader.PropertyToID("_PlotVectors"),
        vector2BufferID = Shader.PropertyToID("_Vectors2"),
        vector3BufferID = Shader.PropertyToID("_Vectors3"),
        magnitudesBufferID = Shader.PropertyToID("_Magnitudes"),
        maxVectorLengthID = Shader.PropertyToID("_MaxVectorLength"),
        //maxMagnitudeID = Shader.PropertyToID("_MaxMagnitude"),
        cullDistanceID = Shader.PropertyToID("_CullDistance"),
        cameraPositionID = Shader.PropertyToID("_CameraPosition");

    /// <summary>
    /// The compute shader used to calculate the object-to-world matrix
    /// </summary>
    [SerializeField]
    ComputeShader displayComputer;

    /// <summary>
    /// Records whether the buffers have been created.
    /// </summary>
    public bool initialized { get; protected set; }

    // = false;
    ///// <summary>
    ///// Records whether the maximum magnitude has been calculated. 
    ///// </summary>
    //protected bool foundMaxMagnitude = false;

    /// <summary>
    /// The distance from the camera inside which vectors are not rendered. 
    /// </summary>
    public float cullDistance;

    // We need a way to set the properties of the different shaders. Material.HasProperty will be useful, plus a custom editor.
    public Shader shader;

    /// <summary>
    /// A delegate called prior to the draw call.
    /// </summary>
    public UnityEvent preDisplay = new UnityEvent();







    private void Awake()
    {
        initialized = false;
    }

    /// <summary>
    /// Can only be run once per enable. Creates the buffers necessary for the display
    /// </summary>
    protected void Initialize()
    {
        if(initialized) { return; }
        unsafe // <-- This could maybe be a source of problems.
        {
            plotVectorsBuffer = new ComputeBuffer(numOfPoints, sizeof(Vector3));
            vector2Buffer = new ComputeBuffer(numOfPoints, sizeof(Vector3));
            vector3Buffer = new ComputeBuffer(numOfPoints, sizeof(Vector3));
            magnitudesBuffer = new ComputeBuffer(numOfPoints, sizeof(float));
            //maxMagnitude = new ComputeBuffer(1, sizeof(float));
        }

        //maxMagnitudeArray = new float[1];

        initialized = true;

        pointerMaterial = new Material(shader);
    }

    // Release the buffers
    private void OnDestroy() // Should this be a disable?
    {
        if(plotVectorsBuffer != null) {
            plotVectorsBuffer.Release();
            plotVectorsBuffer = null;
        }
        if(vector2Buffer != null) {
            vector2Buffer.Release();
            vector2Buffer = null;
        }
        if(vector3Buffer != null) {
            vector3Buffer.Release();
            vector3Buffer = null;
        }
        if (magnitudesBuffer != null)
        {
            magnitudesBuffer.Release();
            magnitudesBuffer = null;
        }
        //if(maxMagnitude != null)
        //{
        //    maxMagnitude.Release();
        //    maxMagnitude = null;
        //}

        //foundMaxMagnitude = false;
        initialized = false;
    }





    /// <summary>
    /// The key function, called by VectorField.
    /// </summary>
    /// <param name="positionsBuffer">A buffer with the positions of each vector.</param>
    /// <param name="vectorsBuffer">A buffer with the values of each vector.</param>
    public override void DisplayVectors(ComputeBuffer positionsBuffer, ComputeBuffer vectorsBuffer)
    {
        numOfPoints = positionsBuffer.count;

        // Make the buffers
        Initialize();

        // Do calculations needed for display
        CalculateDisplay(positionsBuffer, vectorsBuffer);

        //if(colorScheme == VectorStyle.MinMaxColors) {
        //    FindMaxMagnitude();
        //}

        if(preDisplay != null)
        {
            preDisplay.Invoke();
        }

        // Send data to the shader.
        PlotResults(positionsBuffer);
    }



    /// <summary>
    /// Calculates the necessary values to display a vector. 
    /// </summary>
    /// <param name="positionsBuffer">A buffer with the positions of each vector.</param>
    /// <param name="vectorsBuffer">A buffer with the values of each vector.</param>
    protected void CalculateDisplay(ComputeBuffer positionsBuffer, ComputeBuffer vectorsBuffer)
    {
        int kernelID = 0;

        displayComputer.SetInt(numPointsID, numOfPoints);

        displayComputer.SetBuffer(kernelID, positionsBufferID, positionsBuffer);
        displayComputer.SetBuffer(kernelID, vectorsBufferID, vectorsBuffer);
        displayComputer.SetBuffer(kernelID, plotVectorsBufferID, plotVectorsBuffer);
        displayComputer.SetBuffer(kernelID, vector2BufferID, vector2Buffer);
        displayComputer.SetBuffer(kernelID, vector3BufferID, vector3Buffer);
        displayComputer.SetBuffer(kernelID, magnitudesBufferID, magnitudesBuffer);
        displayComputer.SetFloat(maxVectorLengthID, maxVectorLength);
        //displayComputer.SetFloat(cullDistanceID, cullDistance);
        //displayComputer.SetVector(cameraPositionID, Camera.main.transform.position);
        // Does not support multiple cameras

        int numGroups = Mathf.CeilToInt(numOfPoints / 64f);
        displayComputer.Dispatch(kernelID, numGroups, 1, 1);
    }



    /// <summary>
    /// Interfaces with the <cref>pointerMaterial</cref> to display the vector field. 
    /// </summary>
    protected virtual void PlotResults(ComputeBuffer positionsBuffer)
    {
        // Then the data from the computeShader is sent to the shader to be rendered.
        pointerMaterial.SetBuffer(positionsBufferID, positionsBuffer);
        pointerMaterial.SetBuffer(plotVectorsBufferID, plotVectorsBuffer);
        pointerMaterial.SetBuffer(vector2BufferID, vector2Buffer);
        pointerMaterial.SetBuffer(vector3BufferID, vector3Buffer);
        //if (colorScheme == VectorStyle.MinMaxColors) {
        //    pointerMaterial.SetBuffer(magnitudesBufferID, magnitudesBuffer);
        //    pointerMaterial.SetFloat(maxMagnitudeID, maxMagnitudeArray[0]);
        //}
        pointerMaterial.SetFloat(cullDistanceID, cullDistance);

        // Setting the bounds and giving a draw call
        Graphics.DrawMeshInstancedProcedural(pointerMesh, 0, pointerMaterial, bounds, numOfPoints);

        {
            //// Debugging code
            //Vector3[] debugArray = new Vector3[numOfPoints];
            //float[] debugArray = new float[numOfPoints];
            //magnitudesBuffer.GetData(debugArray);
            //Debug.Log((("First three points in magnitude array: " + debugArray[0]) + debugArray[1]) + debugArray[2]);
            //Debug.Log((("Last three points in magnitude array: " + debugArray[numOfPoints - 1]) + debugArray[numOfPoints - 2]) + debugArray[numOfPoints - 3]);
        }
    }



    ///// <summary>
    ///// Finds the maximum magnitude of any of the vectors (used for color bounding).
    ///// </summary>
    //public void FindMaxMagnitude()
    //{
    //    if (foundMaxMagnitude) { return; }
    //    // Calculating the largest vector magnitude.
    //    int magnitudeKernel = 1;
    //    displayComputer.SetBuffer(magnitudeKernel, magnitudesBufferID, magnitudesBuffer);
    //    displayComputer.SetBuffer(magnitudeKernel, maxMagnitudeID, maxMagnitude);
    //    displayComputer.Dispatch(magnitudeKernel, 1, 1, 1);

    //    maxMagnitude.GetData(maxMagnitudeArray);

    //    //// Debug code
    //    //float[] magnitudesArray = new float[numOfPoints];
    //    //magnitudesBuffer.GetData(magnitudesArray);

    //    foundMaxMagnitude = true;
    //}



    ///// <summary>
    ///// Recalculates the maximum magnitude in this frame. 
    ///// </summary>
    //public void RecalculateMaxMagnitude()
    //{
    //    foundMaxMagnitude = false;
    //    FindMaxMagnitude();
    //}
}
