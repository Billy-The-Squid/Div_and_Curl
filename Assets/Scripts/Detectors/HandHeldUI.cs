using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.XR.Interaction.Toolkit;

public class HandHeldUI : MonoBehaviour
{
    /// <summary>
    /// Is the UI currently measuring the output of a device?
    /// </summary>
    protected bool measuring;
    /// <summary>
    /// A reference to the device the UI is reading from. 
    /// </summary>
    protected FieldDetector _detector = null;
    public FieldDetector detector
    {
        get => _detector;
        set
        {
            if(_detector != value)
            {
                _detector = value;
                isVisible = (value != null);
            }
        }
    }

    // The parts of the display itself. 
    /// <summary>
    /// The canvas with the text readout. 
    /// </summary>
    [SerializeField]
    Canvas canvas;
    /// <summary>
    /// The mesh renderer hosting the display. 
    /// </summary>
    [SerializeField]
    MeshRenderer meshRenderer;

    public XRGrabInteractable grabComponent;
    /// <summary>
    /// The display with the name of the output value.
    /// </summary>
    [SerializeField]
    TextMeshProUGUI nameDisplay;
    /// <summary>
    /// The display with the number value. 
    /// </summary>
    [SerializeField]
    protected TextMeshProUGUI numberDisplay;

    /// <summary>
    /// The hand in which the detector should be. 
    /// 
    /// This should be opposite the hand to which the UI is attached. 
    /// </summary>
    [SerializeField]
    XRDirectInteractor hand;

    /// <summary>
    /// The camera that the display centers itself on. 
    /// </summary>
    public Camera eyes;

    /// <summary>
    /// Whichever object controlls rotation.
    /// </summary>
    public Transform pivot;

    public bool showMesh = false;
    protected bool _isVisible;
    protected bool isVisible
    {
        get => _isVisible;
        set
        {            
            _isVisible = value;
            canvas.enabled = value;
            grabComponent.enabled = value;
            if(showMesh)
            {
                meshRenderer.enabled = value;
            }
        }
    }




    private void Start()
    {
        if (!showMesh) meshRenderer.enabled = false;
        isVisible = true;
        isVisible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //CheckForGrabbed(); // This could be bound to OnSelect or something. 

        if (isVisible)
        {
            UpdateDisplay();
        }

        pivot.forward = -(transform.position - eyes.transform.position).normalized;
    }



    /// <summary>
    /// Updates the display.
    /// 
    /// Assumes that <cref>detector</cref> is not null and that display is enabled. 
    /// </summary>
    private void UpdateDisplay()
    {
        //display.SetText(detector.quantityName + ": \n{0:0}", detector.detectorOutput);
        nameDisplay.SetText(detector.detectorReadout.GetName() + ":");
        numberDisplay.SetText(detector.detectorReadout.GetReadout());
    }
}
