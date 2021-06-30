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
    protected FieldDetector detector;

    /// <summary>
    /// The display with the text readout or output.
    /// </summary>
    [SerializeField]
    TextMeshProUGUI display;

    /// <summary>
    /// The hand in which the detector should be. 
    /// 
    /// This should be opposite the hand to which the UI is attached. 
    /// </summary>
    [SerializeField]
    XRDirectInteractor otherHand;





    // Update is called once per frame
    void Update()
    {
        CheckForGrabbed(); // This could be bound to OnSelect or something. 

        if(measuring)
        {
            UpdateDisplay();
        }
    }

    

    /// <summary>
    /// This method checks the opposite hand for a grabbed flux detector and stores it as <cref>detector</cref>.
    /// </summary>
    private void CheckForGrabbed()
    {
        bool grabbing = otherHand.isSelectActive; // Confirm that this does what we want. 

        if(grabbing && !measuring)
        {
            if (otherHand.selectTarget.GetComponent<FieldDetector>()) // Is this even a valid check?
            {
                SetDetector(otherHand.selectTarget.GetComponent<FieldDetector>());
            }
        }
    }



    /// <summary>
    /// Updates the display.
    /// 
    /// Assumes that <cref>detector</cref> is not null and that display is enabled. 
    /// </summary>
    private void UpdateDisplay()
    {
        display.SetText(detector.quantityName + ": \n{0:0}" + detector.detectorOutput);
    }



    /// <summary>
    /// Call this when the UI starts reading values from a detector.
    /// </summary>
    /// <param name="measuredDetector">The detector to be reading from.</param>
    public void SetDetector(FieldDetector measuredDetector)
    {
        measuring = true;
        detector = measuredDetector;
    }



    /// <summary>
    /// Call this when the UI is no longer reading values from a detector. 
    /// </summary>
    public void UnsetDetector()
    {
        measuring = false;
        detector = null;
    }
}
