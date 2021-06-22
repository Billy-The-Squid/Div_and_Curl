using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;


/// <summary>
/// This is an alternative to the default teleportation system. It initializes, enabling the ray interactor, 
/// when the user triggers the "Teleport Start" action, and teleports the user (if the position is valid) when 
/// the action is ended. 
/// </summary>
public class TeleportManager : MonoBehaviour
{
    /// <summary>
    /// The input action asset storing the "Teleport Start" action.
    /// </summary>
    [SerializeField]
    InputActionAsset actionAsset;

    /// <summary>
    /// The "Teleport Start" action. 
    /// </summary>
    private InputAction teleportAction;

    private enum Hand { right, left }
    /// <summary>
    /// Which hand triggers the teleportation?
    /// </summary>
    [SerializeField]
    Hand hand;
    private string handName;

    /// <summary>
    /// The <cref>XRRayInteractor</cref> with which the user selects a teleport location. 
    /// </summary>
    [SerializeField]
    XRRayInteractor rayInteractor;

    /// <summary>
    /// The <cref>TeleportatioProvider</cref> used to move the player. 
    /// </summary>
    [SerializeField]
    TeleportationProvider provider;

    /// <summary>
    /// Determines whether teleportation is currently available.
    /// </summary>
    public bool canTeleport;

    void Start()
    {
        handName = HandToHand(hand);
        string actionMapName = "XRI " + handName + "Hand";
        teleportAction = actionAsset.FindActionMap(actionMapName).FindAction("Teleport Start");
        // MAKE THIS FLEXIBLE, somehow. 

        teleportAction.started += OnTeleportStart;
        teleportAction.canceled += OnTeleportRelease;

        rayInteractor.enabled = false;
    }



    /// <summary>
    /// Changes the <cref>Hand</cref> enum type to a string. Very poorly implemented. 
    /// </summary>
    /// <param name="hand">The <cref>Hand</cref> object selected.</param>
    /// <returns>The string corresponding to <cref>hand</cref>.</returns>
    private string HandToHand(Hand hand)
    {
        if(hand == (Hand)0)
        {
            return "Right";
        } else
        {
            return "Left";
        }
    }

    /// <summary>
    /// Enables <cref>rayInteractor</cref>.
    /// </summary>
    /// <param name="context"></param>
    void OnTeleportStart(InputAction.CallbackContext context)
    {
        //Debug.Log("OnTeleportStart called");
        if(canTeleport)
        {
            rayInteractor.enabled = true;
        }
    }

    /// <summary>
    /// Verifies that the teleport is valid and initiates it. 
    /// </summary>
    /// <param name="context"></param>
    void OnTeleportRelease(InputAction.CallbackContext context)
    {
        //Debug.Log("OnTeleportRelease called");

        // Check that the interactor is enabled and pointing at something. 
        if(!(rayInteractor.enabled)) {
            return;
        }
        if (!rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit)) {
            rayInteractor.enabled = false;
            return;
        }

        // Verify that the user can teleport to the selected location. 
        Vector3 destination; // = transform.position;
        TeleportationAnchor anchor = hit.transform.GetComponent<TeleportationAnchor>();
        if (anchor) {
            destination = anchor.teleportAnchorTransform.position;
        }
        else if (hit.transform.GetComponent<TeleportationArea>()) {
            destination = hit.point;
        }
        else {
            rayInteractor.enabled = false;
            return;
        }

        // Create and file a teleportation request. 
        TeleportRequest request = new TeleportRequest() {
            destinationPosition = destination,
        };
        provider.QueueTeleportRequest(request);

        rayInteractor.enabled = false;
    }
}
