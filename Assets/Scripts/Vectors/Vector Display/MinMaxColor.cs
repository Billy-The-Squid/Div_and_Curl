using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMaxColor : ColorScheme
{
    /// <summary>
    /// The <see cref="VectorField"/> generating the vectors.
    /// </summary>
    public VectorField field;
    // This won't work for the Pointer field, will it?

    /// <summary>
    /// The <see cref="ComputeShader"/> used to calculate magnitudes.
    /// </summary>
    public ComputeShader computer;

    ///// <summary>
    ///// Stores the magnitudes of the vectors in <see cref="display.vectorsBuffer"/>. 
    ///// Same indexing scheme as <cref>positionsBuffer</cref>.
    ///// </summary>
    //public ComputeBuffer magnitudesBuffer { get; protected set; }
    /// <summary>
    /// A single-entry float buffer storing the maximum magnitude found.
    /// </summary>
    protected ComputeBuffer maxMagnitude;
    protected float[] maxMagnitudeArray;

    /// <summary>
    /// Records whether the maximum magnitude has been calculated. 
    /// </summary>
    protected bool foundMaxMagnitude = false;
    /// <summary>
    /// Records whether the buffers have been initialized.
    /// </summary>
    protected bool initialized = false;

    protected int numOfPoints;




    private void Start()
    {
        if(display == null) {
            display = GetComponent<VectorDisplay>();
        }
        // The display should now be able to call ColorMaterial;
        display.preDisplay.AddListener(ColorMaterial);

        Debug.Log("Please remember to check that the event has a reference to our function");
    }

    protected void Initialize()
    {
        if (initialized) return;

        Debug.Log("Initializing MinMax");

        numOfPoints = display.plotVectorsBuffer.count;

        // Set the buffers
        //magnitudesBuffer = new ComputeBuffer(numOfPoints, sizeof(float));
        maxMagnitude = new ComputeBuffer(1, sizeof(float));
        // And the temporary array.
        maxMagnitudeArray = new float[1];

        initialized = true;
    }

    private void OnDestroy()
    {
        //if (magnitudesBuffer != null)
        //{
        //    magnitudesBuffer.Release();
        //    magnitudesBuffer = null;
        //}
        if (maxMagnitude != null)
        {
            maxMagnitude.Release();
            maxMagnitude = null;
        }

        initialized = false;
        foundMaxMagnitude = false;
    }


    /// <inheritdoc/>
    public override void ColorMaterial()
    {
        Debug.Log("Coloring material");

        // Do calculations
        FindMaxMagnitude();

        // Add the important stuff to the material.
        display.pointerMaterial.SetBuffer("_Magnitudes", display.magnitudesBuffer);
        display.pointerMaterial.SetFloat("_MaxMagnitude", maxMagnitudeArray[0]);

        Debug.LogWarning("MinMax does not currently contribute any color to the material");
    }


    /// <summary>
    /// Finds the maximum magnitude of any of the vectors (used for color bounding).
    /// </summary>
    public void FindMaxMagnitude()
    {
        if (foundMaxMagnitude) { return; }

        Debug.Log("Finding the max magnitude");

        //// Calculating the vector magnitudes
        //int kernelID = 0;
        //computer.SetInt("_NumberOfPoints", numOfPoints);
        //computer.SetBuffer(kernelID, "_Vectors", field.vectorsBuffer);
        //computer.SetBuffer(kernelID, "_Magnitudes", magnitudesBuffer);

        //int numGroups = Mathf.CeilToInt(numOfPoints / 64f);
        //computer.Dispatch(kernelID, numGroups, 1, 1);

        // Calculating the largest vector magnitude.
        int magnitudeKernel = 0; //1;
        computer.SetBuffer(magnitudeKernel, "_Magnitudes", display.magnitudesBuffer);
        computer.SetBuffer(magnitudeKernel, "_MaxMagnitude", maxMagnitude);
        computer.Dispatch(magnitudeKernel, 1, 1, 1);

        maxMagnitude.GetData(maxMagnitudeArray);

        //// Debug code
        //float[] magnitudesArray = new float[numOfPoints];
        //magnitudesBuffer.GetData(magnitudesArray);

        foundMaxMagnitude = true;
    }

    /// <summary>
    /// Recalculates the maximum magnitude in this frame. 
    /// </summary>
    public void RecalculateMaxMagnitude()
    {
        foundMaxMagnitude = false;
        FindMaxMagnitude();
    }
}
