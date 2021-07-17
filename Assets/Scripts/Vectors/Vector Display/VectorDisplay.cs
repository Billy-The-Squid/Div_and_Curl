using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VectorDisplay : Display
{
    /// <summary>
    /// The buffer in which the visual magnitudes of each vector are stored. 
    /// Same indexing scheme as <see cref="positionsBuffer"/>.
    /// </summary>
    public ComputeBuffer PlotVectorsBuffer { get; protected set; }
    /// <summary>
    /// One of two buffers in which values used for calculating the transformation matrix for vectors are stored.
    /// Same indexing scheme as <cref>positionsBuffer</cref>.
    /// 
    /// Contains vectors orthogonal to those in <cref>plotVectorsBuffer</cref>, with the same magnitude, in order 
    /// to generate an orthogonal basis. 
    /// </summary>
    public ComputeBuffer Vector2Buffer { get; protected set; }
    /// <summary>
    /// One of two buffers in which values used for calculating the transformation matrix for vectors are stored.
    /// Same indexing scheme as <cref>positionsBuffer</cref>.
    /// 
    /// Contains vectors orthogonal to those in <cref>plotVectorsBuffer</cref>, with the same magnitude, in order 
    /// to generate an orthogonal basis. 
    /// </summary>
    public ComputeBuffer Vector3Buffer { get; protected set; }


    /// <summary>
    /// Stores the magnitudes of the vectors in <cref>vectorsBuffer</cref>. 
    /// Same indexing scheme as <cref>positionsBuffer</cref>.
    /// </summary>
    public ComputeBuffer MagnitudesBuffer { get; protected set; }

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
        cullDistanceID = Shader.PropertyToID("_CullDistance"),
        cameraPositionID = Shader.PropertyToID("_CameraPosition");

    /// <summary>
    /// The compute shader used to calculate the object-to-world matrix
    /// </summary>
    public ComputeShader displayComputer;

    /// <summary>
    /// Records whether the buffers have been created.
    /// </summary>
    public bool Initialized { get; protected set; }

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
        Initialized = false;
    }

    /// <summary>
    /// Can only be run once per enable. Creates the buffers necessary for the display
    /// </summary>
    protected void Initialize()
    {
        if(Initialized) { return; }
        unsafe // <-- This could maybe be a source of problems.
        {
            PlotVectorsBuffer = new ComputeBuffer(numOfPoints, sizeof(Vector3));
            Vector2Buffer = new ComputeBuffer(numOfPoints, sizeof(Vector3));
            Vector3Buffer = new ComputeBuffer(numOfPoints, sizeof(Vector3));
            MagnitudesBuffer = new ComputeBuffer(numOfPoints, sizeof(float));
        }

        Initialized = true;

        pointerMaterial = new Material(shader);
    }

    // Release the buffers
    private void OnDestroy() // Should this be a disable?
    {
        if(PlotVectorsBuffer != null) {
            PlotVectorsBuffer.Release();
            PlotVectorsBuffer = null;
        }
        if(Vector2Buffer != null) {
            Vector2Buffer.Release();
            Vector2Buffer = null;
        }
        if(Vector3Buffer != null) {
            Vector3Buffer.Release();
            Vector3Buffer = null;
        }
        if (MagnitudesBuffer != null)
        {
            MagnitudesBuffer.Release();
            MagnitudesBuffer = null;
        }
        Initialized = false;
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

        if(preDisplay != null) {
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
        displayComputer.SetBuffer(kernelID, plotVectorsBufferID, PlotVectorsBuffer);
        displayComputer.SetBuffer(kernelID, vector2BufferID, Vector2Buffer);
        displayComputer.SetBuffer(kernelID, vector3BufferID, Vector3Buffer);
        displayComputer.SetBuffer(kernelID, magnitudesBufferID, MagnitudesBuffer);
        displayComputer.SetFloat(maxVectorLengthID, maxVectorLength);

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
        pointerMaterial.SetBuffer(plotVectorsBufferID, PlotVectorsBuffer);
        pointerMaterial.SetBuffer(vector2BufferID, Vector2Buffer);
        pointerMaterial.SetBuffer(vector3BufferID, Vector3Buffer);

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
}
