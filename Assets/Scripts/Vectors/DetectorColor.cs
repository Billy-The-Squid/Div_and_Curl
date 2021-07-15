using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorColor : ColorScheme
{
    public FieldDetector detector; 

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
        if (display == null)
        {
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

        numOfPoints = display.PlotVectorsBuffer.count;

        initialized = true;
    }

    private void OnDestroy()
    {
        initialized = false;
    }


    /// <inheritdoc/>
    public override void ColorMaterial()
    {
        //Debug.Log("Coloring material");

        Initialize();

        // Add the important stuff to the material.
        display.pointerMaterial.SetBuffer("_Magnitudes", display.MagnitudesBuffer);
        try
        {
            display.pointerMaterial.SetFloat("_MaxMagnitude", detector.detectedField.GetComponent<MinMaxColor>().maxMagnitudeArray[0]);
        } catch (System.NullReferenceException)
        {
            Debug.LogError("Something here is throwing null reference exceptions. It could really be anything.");
        }

        display.pointerMaterial.SetColor("_MinColor", minColor);
        display.pointerMaterial.SetColor("_MaxColor", maxColor);
    }
}
