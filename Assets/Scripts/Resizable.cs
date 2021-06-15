using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Resizable : MonoBehaviour
{
    [SerializeField]
    private float minRad = 0.05f, maxRad = 1f;

    [SerializeField] // Find some way to clamp here
    public float radius;

    private bool isGrabbed;

    [SerializeField]
    InputActionAsset inputActions;

    private InputAction resizeAction;

    private float lastResized;

    private void Start()
    {
        lastResized = Time.time;

        radius = Mathf.Clamp(radius, minRad, maxRad);
        transform.localScale = Vector3.one * radius;
        isGrabbed = false;

        resizeAction = inputActions.FindActionMap("XRI RightHand").FindAction("Resize Object");
    }

    private void Update()
    {
        if (isGrabbed && (Time.time > lastResized + 0.5) && (resizeAction.phase == InputActionPhase.Started))
        {
            float current = resizeAction.ReadValue<Vector2>().y;
            if (current > 0f)
            {
                SizeUp();
                lastResized = Time.time;
            } else if(current < 0f)
            {
                SizeDown();
                lastResized = Time.time;
            }
        }
    }

    public void SetGrabbedFalse ()
    {
        isGrabbed = false;
    }

    public void SetGrabbedTrue()
    {
        isGrabbed = true;
    }

    public void SizeUp()
    {
        radius = Mathf.Clamp(radius + 0.1f, minRad, maxRad);
        transform.localScale = Vector3.one * radius;
    }

    public void SizeDown()
    {
        radius = Mathf.Clamp(radius - 0.1f, minRad, maxRad);
        transform.localScale = Vector3.one * radius;
    }
}
