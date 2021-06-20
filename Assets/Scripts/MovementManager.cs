using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Switches between different movement modes. May be set by te user. 
/// </summary>
public class MovementManager : MonoBehaviour
{
    /// <summary>
    /// The continuous movement provider. 
    /// </summary>
    [SerializeField]
    ContinuousMoveProviderBase moveProvider;
    /// <summary>
    /// The teleport manager. 
    /// </summary>
    [SerializeField]
    TeleportManager teleporter;

    /// <summary>
    /// The types of directional movement available. 
    /// </summary>
    private enum MovementMode { Smooth, Teleport }
    /// <summary>
    /// The method of directional movement. 
    /// </summary>
    [SerializeField]
    MovementMode movementMode = MovementMode.Teleport;

    

    void Start()
    {
        UpdateMovementMode();
    }



    /// <summary>
    /// Enables and disables movement components based on the movement mode selected. 
    /// </summary>
    public void UpdateMovementMode()
    {
        if(movementMode == MovementMode.Smooth) {
            moveProvider.enabled = true;
            teleporter.enabled = false;
        } else {
            moveProvider.enabled = false;
            teleporter.enabled = true;
        }
    }



    /// <summary>
    /// Sets the directional movement mode to "Smooth."
    /// </summary>
    public void SetModeSmooth()
    {
        movementMode = MovementMode.Smooth;
        UpdateMovementMode();
    }

    /// <summary>
    /// Sets the directional movement mode to "Teleport."
    /// </summary>
    public void SetModeTeleport()
    {
        movementMode = MovementMode.Teleport;
        UpdateMovementMode();
    }
}
