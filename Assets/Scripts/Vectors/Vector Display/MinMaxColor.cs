using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMaxColor : ColorScheme
{
    ///// <summary>
    ///// The <see cref="VectorField"/> generating the vectors.
    ///// </summary>
    //public VectorField field;
    //// This won't work for the Pointer field, will it? Or maybe I'm thinking about the loop axis.

    /// <summary>
    /// The <see cref="ComputeShader"/> used to calculate magnitudes.
    /// </summary>
    public ComputeShader computer;

    /// <summary>
    /// A single-entry float buffer storing the maximum magnitude found.
    /// </summary>
    protected ComputeBuffer maxMagnitude;
    public float[] maxMagnitudeArray { get; protected set; }

    /// <summary>
    /// Records whether the maximum magnitude has been calculated. 
    /// </summary>
    protected bool foundMaxMagnitude = false;
    /// <summary>
    /// Records whether the buffers have been initialized.
    /// </summary>
    protected bool initialized = false;

    protected int numOfPoints;

    [Tooltip("The color to display the largest-magnitude vectors.")]
    public Color maxColor = Color.yellow;
    [Tooltip("The color to display the vectors with magnitude near zero.")]
    public Color minColor = Color.blue;





    private void Start()
    {
        if(display == null) {
            display = GetComponent<VectorDisplay>();
        }
        //// I guess we just do this in the inspector?
        //// The display should now be able to call ColorMaterial;
        //display.preDisplay.AddListener(ColorMaterial);

        //Debug.Log("Please remember to check that the event has a reference to our function");
    }

    protected void Initialize()
    {
        if (initialized) return;

        //Debug.Log("Initializing MinMax");

        numOfPoints = display.PlotVectorsBuffer.count;

        // Set the buffer
        maxMagnitude = new ComputeBuffer(1, sizeof(float));
        // And the temporary array.
        maxMagnitudeArray = new float[1];

        initialized = true;
    }

    private void OnDestroy() {
        if (maxMagnitude != null) {
            maxMagnitude.Release();
            maxMagnitude = null;
        }

        initialized = false;
        foundMaxMagnitude = false;
    }


    /// <inheritdoc/>
    public override void ColorMaterial()
    {
        //Debug.Log("Coloring material");

        Initialize();

        // Do calculations
        FindMaxMagnitude();

        // Add the important stuff to the material.
        display.pointerMaterial.SetBuffer("_Magnitudes", display.MagnitudesBuffer);
        display.pointerMaterial.SetFloat("_MaxMagnitude", maxMagnitudeArray[0]);

        display.pointerMaterial.SetColor("_MinColor", minColor);
        display.pointerMaterial.SetColor("_MaxColor", maxColor);
    }


    /// <summary>
    /// Finds the maximum magnitude of any of the vectors (used for color bounding).
    /// </summary>
    public void FindMaxMagnitude()
    {
        if (foundMaxMagnitude) { return; }

        //Debug.Log("Finding the max magnitude");

        // Calculating the largest vector magnitude.
        int magnitudeKernel = 0;
        computer.SetInt("_NumberOfPoints", numOfPoints);
        computer.SetBuffer(magnitudeKernel, "_Magnitudes", display.MagnitudesBuffer);
        computer.SetBuffer(magnitudeKernel, "_MaxMagnitude", maxMagnitude);
        computer.Dispatch(magnitudeKernel, 1, 1, 1);

        maxMagnitude.GetData(maxMagnitudeArray);

        // Debug code
        float[] magnitudesArray = new float[numOfPoints];
        display.MagnitudesBuffer.GetData(magnitudesArray);

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
