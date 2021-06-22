using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandMonitor : MonoBehaviour
{
    [SerializeField]
    InputActionAsset asset;
    InputAction action;
    InputAction action2;

    // Start is called before the first frame update
    void Start()
    {
        action = asset.FindActionMap("XRI RightHand").FindAction("Select");
        action2 = asset.FindActionMap("XRI LeftHand").FindAction("Select");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Action value: " + action.phase);
        Debug.Log("Left phase: " + action2.phase);
    }
}
