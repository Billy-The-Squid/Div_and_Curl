using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class DetectorMenuUI : MonoBehaviour
{
    /// <summary>
    /// The object to be facing
    /// </summary>
    public Transform playerEyes;

    /// <summary> 
    /// The canvas with all the relevant information.
    /// </summary>
    public Canvas canvas;

    /// <summary>
    /// The display text bearing the detector name. 
    /// </summary>
    public TextMeshProUGUI detectorName;

    /// <summary>
    /// The socket into which detectors will be placed.
    /// </summary>
    public XRSocketInteractor socket;






    // Update is called once per frame
    void Update()
    {
        ReactToPlayer();

        // Only display names if the object socketed is a detector. 
        if(socket.selectTarget != null && socket.selectTarget.GetComponent<FieldDetector>() != null) {
            detectorName.SetText(socket.selectTarget.GetComponent<FieldDetector>().displayName);
        } 
        else {
            canvas.enabled = false;
        }

        
    }

    /// <summary>
    /// Rotates to face the player and closes the display if the player is far enough away. 
    /// </summary>
    protected void ReactToPlayer()
    {
        // Rotate to face the player
        Vector3 displacement = transform.position - playerEyes.position;
        Vector3 planeDistance = new Vector3(displacement.x, 0, displacement.z);
        transform.forward = planeDistance.normalized;

        // Close the display if the player is far away. 
        if(planeDistance.magnitude <= 1f) {
            canvas.enabled = true;
        } else {
            canvas.enabled = false;
        }
    }


}
