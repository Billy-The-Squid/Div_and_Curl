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

    // Start is called before the first frame update
    void Start()
    {
        handName = HandToHand(hand);
        string actionMapName = "XRI " + handName + "Hand";
        //Debug.Log("Searching for: " + actionMapName);
        teleportAction = actionAsset.FindActionMap(actionMapName).FindAction("Teleport Start");
        // MAKE THIS FLEXIBLE
        //Debug.Log("Action found: " + (teleportAction != null));
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
