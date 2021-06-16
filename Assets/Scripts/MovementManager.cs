using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MovementManager : MonoBehaviour
{
    [SerializeField]
    ContinuousMoveProviderBase moveProvider;
    [SerializeField]
    TeleportManager teleporter;

    private enum MovementMode { Smooth, Teleport }
    [SerializeField]
    MovementMode movementMode = MovementMode.Teleport;

    // Start is called before the first frame update
    void Start()
    {
        UpdateMovementMode();
    }

    public void UpdateMovementMode()
    {
        if(movementMode == MovementMode.Smooth)
        {
            moveProvider.enabled = true;
            teleporter.enabled = false;
        } else
        {
            moveProvider.enabled = false;
            teleporter.enabled = true;
        }
    }

    public void SetModeSmooth()
    {
        movementMode = MovementMode.Smooth;
        UpdateMovementMode();
    }

    public void SetModeTeleport()
    {
        movementMode = MovementMode.Teleport;
        UpdateMovementMode();
    }
}
