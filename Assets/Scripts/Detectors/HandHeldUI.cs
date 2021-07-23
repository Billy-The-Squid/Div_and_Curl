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

    

    ///// <summary>
    ///// This method checks the opposite hand for a grabbed flux detector and stores it as <cref>detector</cref>.
    ///// </summary>
    //public void CheckForGrabbed()
    //{
    //    bool grabbing = hand.isSelectActive; // Is true whenever the action itself is active

    //    // If the hand is grabbing but there's nothing we're measuring, check what's being held. 
    //    if(grabbing && !measuring)
    //    {
    //        // Getting a null ref error here sometimes. 
    //        if (hand.selectTarget != null && hand.selectTarget.GetComponent<FieldDetector>()) // Is this even a valid check?
    //        {
    //            SetDetector(hand.selectTarget.GetComponent<FieldDetector>());
    //        }
    //    } else if (!grabbing || !measuring)
    //    {
    //        UnsetDetector();
    //    }
    //}



    /// <summary>
    /// Updates the display.
    /// 
    /// Assumes that <cref>detector</cref> is not null and that display is enabled. 
    /// </summary>
    private void UpdateDisplay()
    {
        //display.SetText(detector.quantityName + ": \n{0:0}", detector.detectorOutput);
        nameDisplay.SetText(detector.quantityName + ":");
        numberDisplay.SetText("{0:1}", detector.detectorOutput);
    }



    ///// <summary>
    ///// Call this when the UI starts reading values from a detector.
    ///// </summary>
    ///// <param name="measuredDetector">The detector to be reading from.</param>
    //public void SetDetector(FieldDetector measuredDetector)
    //{
    //    measuring = true;
    //    detector = measuredDetector;
    //    isVisible = true;
    //}



    ///// <summary>
    ///// Call this when the UI is no longer reading values from a detector. 
    ///// </summary>
    //public void UnsetDetector()
    //{
    //    measuring = false;
    //    detector = null;
    //    isVisible = false;
    //}
}
