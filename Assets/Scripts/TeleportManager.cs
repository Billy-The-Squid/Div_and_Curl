using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportManager : MonoBehaviour
{
    [SerializeField]
    InputActionAsset actionAsset;

    private InputAction teleportAction;

    private enum Hand { right, left }
    [SerializeField]
    Hand hand;
    private string handName;

    [SerializeField]
    XRRayInteractor rayInteractor;

    [SerializeField]
    TeleportationProvider provider;

    // Start is called before the first frame update
    void Start()
    {
        handName = HandToHand(hand);
        string actionMapName = "XRI " + handName + "Hand";
        //Debug.Log("Searching for Action Map " + actionMapName);
        teleportAction = actionAsset.FindActionMap(actionMapName).FindAction("Teleport Start");
        // MAKE THIS FLEXIBLE, somehow. 
        //Debug.Log("Found action: " + (teleportAction != null));

        teleportAction.started += OnTeleportStart;
        teleportAction.canceled += OnTeleportRelease;

        rayInteractor.enabled = false;
    }



    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Phase: " + teleportAction.phase);
    }



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

    void OnTeleportStart(InputAction.CallbackContext context)
    {
        Debug.Log("OnTeleportStart called");
        rayInteractor.enabled = true;
    }

    void OnTeleportRelease(InputAction.CallbackContext context)
    {
        Debug.Log("OnTeleportRelease called");

        if(!(rayInteractor.enabled))
        {
            return;
        }

        if (!rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            rayInteractor.enabled = false;
            return;
        }

        Vector3 destination; // = transform.position;
        TeleportationAnchor anchor = hit.transform.GetComponent<TeleportationAnchor>();
        if (anchor)
        {
            destination = anchor.teleportAnchorTransform.position;
        }
        else if (hit.transform.GetComponent<TeleportationArea>())
        {
            destination = hit.point;
        }
        else
        {
            rayInteractor.enabled = false;
            return;
        }

        TeleportRequest request = new TeleportRequest()
        {
            destinationPosition = destination,
        };

        provider.QueueTeleportRequest(request);

        rayInteractor.enabled = false;
    }
}
