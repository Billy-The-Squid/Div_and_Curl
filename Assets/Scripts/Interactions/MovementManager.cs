using System;
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
    /// The <cref>TeleportProvider</cref>. 
    /// </summary>
    [SerializeField]
    TeleportationProvider teleporter;
    /// <summary>
    /// The <cref>TeleportManager</cref>.
    /// </summary>
    [SerializeField]
    TeleportManager teleportManager;

    /// <summary>
    /// The continuous turn provider. 
    /// </summary>
    [SerializeField]
    ContinuousTurnProviderBase smoothTurnProvider;
    /// <summary>
    /// The snap turn provider.
    /// </summary>
    [SerializeField]
    SnapTurnProviderBase snapTurnProvider;



    /// <summary>
    /// The types of directional movement available. 
    /// </summary>
    private enum MovementMode { Smooth, Teleport }
    /// <summary>
    /// The method of directional movement. 
    /// </summary>
    [SerializeField]
    MovementMode movementMode = MovementMode.Teleport;

    /// <summary>
    /// The types of turning available.
    /// </summary>
    private enum TurnMode { Smooth, Snap }
    /// <summary>
    /// The turn method.
    /// </summary>
    [SerializeField]
    TurnMode turnMode = TurnMode.Snap;

    

    void Start()
    {
        UpdateMovementMode();
        UpdateTurnMode();
    }





    /// <summary>
    /// Enables and disables movement components based on the movement mode selected. 
    /// </summary>
    public void UpdateMovementMode()
    {
        if(movementMode == MovementMode.Smooth) {
            moveProvider.enabled = true;
            teleporter.enabled = false;
            teleportManager.canTeleport = false;
        } else {
            moveProvider.enabled = false;
            teleporter.enabled = true;
            teleportManager.canTeleport = true;
        }
    }

    /// <summary>
    /// Enables and disables turn components based on the turn mode selected. 
    /// </summary>
    private void UpdateTurnMode()
    {
        if (turnMode == TurnMode.Smooth)
        {
            smoothTurnProvider.enabled = true;
            snapTurnProvider.enabled = false;
        }
        else
        {
            smoothTurnProvider.enabled = false;
            snapTurnProvider.enabled = true;
        }
    }



    /// <summary>
    /// Sets the directional movement mode to "Smooth."
    /// </summary>
    public void SetMoveModeSmooth()
    {
        movementMode = MovementMode.Smooth;
        UpdateMovementMode();
    }

    /// <summary>
    /// Sets the directional movement mode to "Teleport."
    /// </summary>
    public void SetMoveModeTeleport()
    {
        movementMode = MovementMode.Teleport;
        UpdateMovementMode();
    }

    /// <summary>
    /// Sets the turn mode to "Smooth."
    /// </summary>
    public void SetTurnModeSmooth()
    {
        turnMode = TurnMode.Smooth;
        UpdateTurnMode();
    }

    /// <summary>
    /// Sets the turn mode to "Snap."
    /// </summary>
    public void SetTurnModeSnap()
    {
        turnMode = TurnMode.Snap;
        UpdateTurnMode();
    }
}
