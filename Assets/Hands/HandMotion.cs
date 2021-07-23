using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandMotion : MonoBehaviour
{
    [Tooltip("The mesh renderer for the hand. Search for it in the hierarchy.")]
    public SkinnedMeshRenderer meshRenderer;

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

    private Animator animator;
    int thumbLayerIndex;
    int pointLayerIndex;

    public bool isPointing = false;



    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Hand detection script is running"); 
        map = actionAsset.FindActionMap("XRI " + hands[(int)hand] + "Hand");
        //Debug.Log("Found map (" + map.name + "):" + (map != null));
        //map.Enable();

        pinch = map.FindAction("Pinch"); // Value type, axis.
        grip = map.FindAction("Grip"); // Value type, axis.
        pinchTouch = map.FindAction("Pinch Touch"); // Button type, float
        thumbTouch = map.FindAction("Thumb Touch"); // Button type, float

        //Debug.Log("Found action: " + pinch.name);
        //Debug.Log("Found action: " + grip.name);
        //Debug.Log("Found action: " + pinchTouch.name);
        //Debug.Log("Found action: " + thumbTouch.name);

        animator = GetComponent<Animator>();
        thumbLayerIndex = animator.GetLayerIndex("Thumb Layer");
        pointLayerIndex = animator.GetLayerIndex("Point Layer");
    }

    // Update is called once per frame
    void Update()
    {
        //DebugAction(pinch);
        animator.SetFloat("Flex", grip.ReadValue<float>());
        animator.SetFloat("Pinch", pinch.ReadValue<float>()); // Does this need modification?

        float pointWeight = isPointing ? 1 : 1f - pinchTouch.ReadValue<float>();

        animator.SetLayerWeight(thumbLayerIndex, 1f - thumbTouch.ReadValue<float>()); // Or is it 1-x?
        animator.SetLayerWeight(pointLayerIndex, pointWeight);

        //Debug.Log("Pinch: " + animator.GetFloat("Pinch"));
        //Debug.Log("Flex: " + animator.GetFloat("Flex"));
    }

    void DebugAction(InputAction action)
    {
        Debug.Log("Map (" + map.name + ") Action (" + action.name + ")");
        Debug.Log("Read value (float): " + action.ReadValue<float>());
        //Debug.Log("Read value (bool): " + action.ReadValue<bool>());
        Debug.Log("Triggered? " + action.triggered);
        Debug.Log("Phase: " + action.phase);
    }

    public void SetVisible(bool visible)
    {
        meshRenderer.enabled = visible;
    }
}
