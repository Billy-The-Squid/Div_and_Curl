using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class DetectorMenuUI : MenuUI
{
    /// <summary>
    /// The list of available detector prefabs.
    /// </summary>
    public Grabbable[] detectorsArray;
    /// <summary>
    /// The list of detectors currently available. Each entry is an index in detectorsArray. 
    /// 
    /// DO NOT repeat entries.
    /// </summary>
    public List<int> availableDetectors;
    /// <summary>
    /// The detectors currently in the scene.
    /// </summary>
    public List<Grabbable> detectorsInScene; // Necessary?


    // Should this be an int?
    /// <summary>
    /// The detector type to be currently displaying.
    /// </summary>
    public int currentlyDisplayedDetector;


    /// <summary>
    /// Indicates that the display is in need of an update.
    /// </summary>
    protected bool displayNeedsUpdate = false;
    // Should be set true if: %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

    /// <summary>
    /// The display text bearing the detector name. 
    /// </summary>
    public TextMeshProUGUI detectorNameDisplay;

    /// <summary>
    /// The socket into which detectors will be placed.
    /// </summary>
    public XRSocketInteractor socket;



    /// <summary>
    /// Is the set of detectors limited?
    /// </summary>
    public bool limitDetectors = false;










    private void Start()
    {
        if(!limitDetectors) {
            List<int> detsList = new List<int>();
            for (int i = 0; i < detectorsArray.Length; i++)
            {
                detsList.Add(i);
            }
            SetAvailableDetectors(detsList);
            currentlyDisplayedDetector = 0;
        } else {
            throw new System.Exception("I don't have a list to limit the detectors to.");
        }

        for(int i = 0; i < detectorsArray.Length; i++) // I really don't know that this is safe.
        {
            detectorsArray[i].menuIndex = i;
        }
    }



    // Update is called once per frame
    void Update()
    {
        ReactToPlayer();

        if (displayNeedsUpdate)
        {
            UpdateDisplay();
        }
    }





    /// <summary>
    /// Rotates to face the player and closes the display if the player is far enough away. 
    /// </summary>
    protected override void ReactToPlayer()
    {
        // Rotate to face the player
        Vector3 displacement = transform.position - playerEyes.position;
        Vector3 planeDistance = new Vector3(displacement.x, 0, displacement.z);
        transform.forward = planeDistance.normalized;

        base.ReactToPlayer();
    }



    /// <summary>
    /// Sets the list of currently available detectors.
    /// </summary>
    /// <param name="detectors">A list containing the prefabs to be made available.</param>
    public void SetAvailableDetectors(List<int> detectors)
    {
        availableDetectors = new List<int>(detectors);
        displayNeedsUpdate = true;
    }



    ///// <summary>
    ///// Prompt the display to update.
    ///// </summary>
    //public void PromptDisplayUpdate()
    //{
    //    displayNeedsUpdate = true;
    //}



    /// <summary>
    /// Set the currently displayed detector to the one that's socketed. If none are socketed, closes display.
    /// </summary>
    public void DisplaySocketed()
    {
        // Only display names if the object socketed is a detector. 
        if (socket.selectTarget != null && socket.selectTarget.GetComponent<FieldDetector>() != null)
        {
            //currentlyDisplayedDetector = 
            
            //detectorNameDisplay.SetText(socket.selectTarget.GetComponent<FieldDetector>().displayName);
        }
        else
        {
            //canvas.enabled = false;
        }

        displayNeedsUpdate = true;
    }



    /// <summary>
    /// Update display.
    /// </summary>
    protected void UpdateDisplay()
    {
        Grabbable currentDetector = detectorsArray[currentlyDisplayedDetector];

        //if (currentDetector.GetComponent<FieldDetector>() != null) 
        {
            detectorNameDisplay.SetText(currentDetector.displayName);
        }
        

        displayNeedsUpdate = false;
    }
}
