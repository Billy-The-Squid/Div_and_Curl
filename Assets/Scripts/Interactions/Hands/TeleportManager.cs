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
    public XRRayInteractor rayInteractor;

    /// <summary>
    /// The <cref>TeleportatioProvider</cref> used to move the player. 
    /// </summary>
    [SerializeField]
    TeleportationProvider provider;

    protected bool _canTeleport;
    /// <summary>
    /// Determines whether teleportation is currently available.
    /// </summary>
    public bool canTeleport
    {
        get => _canTeleport;
        set
        {
            _canTeleport = value;
            if(!value && rayInteractor.enabled)
            {
                EndTeleport();
            }
        }
    }

    [SerializeField]
    GameObject reticulePrefab;
    GameObject reticuleInstance;




    void Start()
    {
        // Find the teleport action.
        handName = HandToHand(hand);
        string actionMapName = "XRI " + handName + "Hand";
        teleportAction = actionAsset.FindActionMap(actionMapName).FindAction("Teleport Start");
        // MAKE THIS FLEXIBLE, somehow. 

        // Bind to the teleport action.
        teleportAction.started += OnTeleportStart;
        teleportAction.canceled += OnTeleportRelease;

        rayInteractor.enabled = false;

        // Set the teleport reticule.
        reticuleInstance = Instantiate(reticulePrefab, transform);
        reticuleInstance.SetActive(false);
    }

    private void Update()
    {
        if(rayInteractor.enabled)
        {
            teleportDestination destination = CheckLocation();
            if (destination.validDestination)
            {
                reticuleInstance.SetActive(true);
                reticuleInstance.transform.position = destination.location;
                reticuleInstance.transform.up = destination.normal;
                // Test this last part...
            } else
            {
                reticuleInstance.SetActive(false);
            }
        } else
        {
            reticuleInstance.SetActive(false);
        }
    }



    /// <summary>
    /// Changes the <cref>Hand</cref> enum type to a string. Very poorly implemented. 
    /// </summary>
    /// <param name="hand">The <see cref="Hand"/> object selected.</param>
    /// <returns>The string corresponding to hand.</returns>
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
        if (canTeleport)
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
        teleportDestination destination = CheckLocation();

        if(destination.validDestination)
        {
            // Create and file a teleportation request. 
            TeleportRequest request = new TeleportRequest()
            {
                destinationPosition = destination.location,
            };
            provider.QueueTeleportRequest(request);
        }

        EndTeleport();
    }



    // Call to stop rayCasting and disappear the reticle. 
    void EndTeleport()
    {
        rayInteractor.enabled = false;
        if(reticuleInstance != null)
        {
            reticuleInstance.SetActive(false);
        }
    }

    struct teleportDestination
    {
        public Vector3 location;
        public bool validDestination;
        public Vector3 normal;
    }

    teleportDestination CheckLocation()
    {
        teleportDestination destination = new teleportDestination();
        destination.validDestination = false;

        // Check that the interactor is enabled and pointing at something. 
        if (!(rayInteractor.enabled))
        {
            return destination;
        }
        if (!rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            //rayInteractor.enabled = false;
            return destination;
        }

        // Verify that the user can teleport to the selected location. 
        //Vector3 destination; // = transform.position;
        TeleportationAnchor anchor = hit.transform.GetComponent<TeleportationAnchor>();
        if (anchor)
        {
            destination.validDestination = true;
            destination.location = anchor.teleportAnchorTransform.position;
            destination.normal = anchor.transform.up;
        }
        else if (hit.transform.GetComponent<TeleportationArea>())
        {
            destination.validDestination = true;
            destination.location = hit.point;
            destination.normal = hit.normal;
        }
        else
        {
            destination.validDestination = false;
            //rayInteractor.enabled = false;
            return destination;
        }

        return destination;
    }
}
