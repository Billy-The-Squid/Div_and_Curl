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
    public GameObject[] detectorsArray;
    /// <summary>
    /// The list of detector prefabs currently available.
    /// </summary>
    public List<GameObject> availableDetectors;
    /// <summary>
    /// The detectors currently in the scene.
    /// </summary>
    public List<GameObject> detectorsInScene; // Necessary?


    // Should this be an int?
    /// <summary>
    /// The detector type to be currently displaying.
    /// </summary>
    public int currentlyDisplayedDetector;



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
            SetAvailableDetectors(detectorsArray);
            currentlyDisplayedDetector = 0;
        } else {
            throw new System.Exception("I don't have a list to limit the detectors to.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        ReactToPlayer();



        // Only display names if the object socketed is a detector. 
        if(socket.selectTarget != null && socket.selectTarget.GetComponent<FieldDetector>() != null) {
            detectorNameDisplay.SetText(socket.selectTarget.GetComponent<FieldDetector>().displayName);
        } 
        else {
            canvas.enabled = false;
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
    /// <param name="detectors">An array containing the prefabs to be made available.</param>
    public void SetAvailableDetectors(GameObject[] detectors)
    {
        availableDetectors = detectors.ToList();
    }
    /// <summary>
    /// Sets the list of currently available detectors.
    /// </summary>
    /// <param name="detectors">A list containing the prefabs to be made available.</param>
    public void SetAvailableDetectors(List<GameObject> detectors)
    {
        availableDetectors = new List<GameObject>(detectors);
    }
}
