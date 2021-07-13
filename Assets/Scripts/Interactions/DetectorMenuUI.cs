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


    /// <summary>
    /// The detector type to be currently displaying.
    /// </summary>
    public int currentlyDisplayedDetector //;
    { get => _currentIndex;
        set { _currentIndex = value;
            displayNeedsUpdate = true; } }

    private int _currentIndex;


    /// <summary>
    /// Indicates that the display is in need of an update.
    /// </summary>
    protected bool displayNeedsUpdate = false;
    /* Should be set true if: 
     *  Going to next or previous detector
     *  De-socketing
     *  Re-socketing (specifically, on hover)
     */

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

    /// <summary>
    /// The amount of time to wait before making a new detector.
    /// </summary>
    public float waitTime = 0.5f;

    ///// <summary>
    ///// Is there an object hovering over the socket?
    ///// </summary>
    //protected bool isHovering = false;










    private void Start()
    {
        if (!limitDetectors) {
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

        for (int i = 0; i < detectorsArray.Length; i++) // I really don't know that this is safe.
        {
            detectorsArray[i].menuIndex = i;
        }

        if(waitTime < socket.recycleDelayTime)
        {
            Debug.LogWarning("Detector menu wait time is less than the socket refresh time.");
        }
    }



    // Update is called once per frame
    void Update()
    {
        ReactToPlayer();

        if (displayNeedsUpdate)
        {
            UpdateDisplay();
            UpdateDetector();
            // MAKE SURE DISPLAYNEEDSUPDATE ONLY RESET AFTER SUCCESSFUL %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        }

        //Debug.Log(socket.isHoverActive ? "Hovering" : "Not hovering");
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
        XRBaseInteractable socketed = socket.selectTarget;

        // Only display names if the object socketed is a detector. 
        if (socketed != null && socketed.GetComponent<Grabbable>() != null) {
            currentlyDisplayedDetector = socketed.GetComponent<Grabbable>().menuIndex;
        }
        else {
            displayNeedsUpdate = true;
        }
    }



    /// <summary>
    /// Updates the display.
    /// </summary>
    protected void UpdateDisplay()
    {
        Grabbable currentDetector = detectorsArray[currentlyDisplayedDetector];

        detectorNameDisplay.SetText(currentDetector.displayName);

        displayNeedsUpdate = false;
    }



    /// <summary>
    /// Retrieves the index of the next detector, or -1 if there is no next detector.
    /// </summary>
    /// <returns>The index of the next available detector, or -1 if there is none.</returns>
    protected int NextDetector()
    {
        int cursor = availableDetectors.IndexOf(currentlyDisplayedDetector);

        if (cursor == -1) {
            throw new System.Exception("That detector isn't in the list of available detectors");
        }

        if (cursor + 1 < availableDetectors.Count) {
            return availableDetectors[cursor + 1];
        }
        else {
            return -1;
        }
    }

    /// <summary>
    /// Retrieves the index of the previous detector, or -1 if there is no previous detector.
    /// </summary>
    /// <returns>The index of the previous available detector, or -1 if there is none.</returns>
    protected int PreviousDetector()
    {
        int cursor = availableDetectors.IndexOf(currentlyDisplayedDetector);

        if (cursor == -1) {
            throw new System.Exception("That detector isn't in the list of available detectors");
        }

        if (cursor - 1 >= 0) {
            return availableDetectors[cursor - 1];
        }
        else {
            return -1;
        }
    }



    /// <summary>
    /// Brings up the next detector in the menu.
    /// </summary>
    public void BringUpNext() {
        int temp = NextDetector();
        if (temp >= 0)
        {
            currentlyDisplayedDetector = temp;
        }
    }

    /// <summary>
    /// Brings up the previous detector in the menu.
    /// </summary>
    public void BringUpPrevious() {
        int temp = PreviousDetector();
        if (temp >= 0)
        {
            currentlyDisplayedDetector = temp;
        }
    }



    /// <summary>
    /// Checks whether the currentDisplayedDetector and the socketed detector are the same. If they are not, destroys the one and creates the other.
    /// </summary>
    protected void UpdateDetector()
    {
        XRBaseInteractable socketed = socket.selectTarget;
        List<XRBaseInteractable> hovered = new List<XRBaseInteractable>();
        Grabbable instantiated = null;


        // Has something just been removed from the socket?
        if(socketed == null)
        {
            // Don't forget to check limits! %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            StartCoroutine(WaitToMake());
        }
        else if (socketed.GetComponent<Grabbable>() != null)
        {
            // Is there something hovering over the socket? &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            socket.GetHoverTargets(hovered); 
            if(hovered.Count != 0)
            {
                // That's not what we just made, right?
                if(hovered.Count != 1 || hovered[0] != socketed)
                {
                    Destroy(socketed.gameObject);
                }
            }


            // Did we just change to the next item in the menu?
            else if (currentlyDisplayedDetector != socketed.GetComponent<Grabbable>().menuIndex) { // Check limits! %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                Destroy(socketed.gameObject);
                Debug.Log("Thing was destroyed: " + (socketed == null));
                instantiated = Instantiate(detectorsArray[currentlyDisplayedDetector]);
                instantiated.transform.position = socket.transform.position;
            }
        }


        IEnumerator WaitToMake()
        {
            yield return new WaitForSeconds(waitTime);

            socket.GetHoverTargets(hovered);

            // If there's nothing in the socket or hovering, refill.
            if (socket.selectTarget == null && hovered.Count == 0)
            {
                instantiated = Instantiate(detectorsArray[currentlyDisplayedDetector]);
                instantiated.transform.position = socket.transform.position;
            }
            else
            {
                Debug.Log("Opted not to instantiate new thing.");
            }
        }
    }
}
