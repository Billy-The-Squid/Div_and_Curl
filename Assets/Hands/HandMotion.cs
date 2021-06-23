using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandMotion : MonoBehaviour
{
    [SerializeField]
    InputActionAsset actionAsset;
    private InputActionMap map;

    private InputAction pinch;
    private InputAction grip;
    private InputAction pinchTouch;
    private InputAction thumbTouch;

    public enum Hand { Right, Left }
    private string[] hands = { "Right", "Left" };
    [SerializeField]
    Hand hand;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hand detection script is running"); 
        map = actionAsset.FindActionMap("XRI " + hands[(int)hand] + "Hand");
        Debug.Log("Found map (" + map.name + "):" + (map != null));
        map.Enable();

        pinch = map.FindAction("Pinch");
        grip = map.FindAction("Grip");
        pinchTouch = map.FindAction("Pinch Touch");
        thumbTouch = map.FindAction("Thumb Touch");

        Debug.Log("Found action: " + pinch.name);
        Debug.Log("Found action: " + grip.name);
        Debug.Log("Found action: " + pinchTouch.name);
        Debug.Log("Found action: " + thumbTouch.name);
    }

    // Update is called once per frame
    void Update()
    {
        DebugAction(pinch);
    }

    void DebugAction(InputAction action)
    {
        Debug.Log("Map (" + map.name + ") Action (" + action.name + ")");
        //Debug.Log("Read value (float): " + action.ReadValue<float>());
        //Debug.Log("Read value (bool): " + action.ReadValue<bool>());
        //Debug.Log("Triggered? " + action.triggered);
        Debug.Log("Phase: " + action.phase);
    }
}
