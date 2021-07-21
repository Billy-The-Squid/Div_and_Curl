using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

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
    public enum MovementMode { Smooth, Teleport }
    /// <summary>
    /// The method of directional movement. 
    /// </summary>
    [SerializeField]
    protected MovementMode _movementMode = MovementMode.Teleport;
    public MovementMode movementMode
    {
        get => _movementMode;
        set
        {
            if(_movementMode != value)
            {
                _movementMode = value;
                UpdateMovementMode();
            }
        }
    }

    /// <summary>
    /// The types of turning available.
    /// </summary>
    public enum TurnMode { Smooth, Snap }
    /// <summary>
    /// The turn method.
    /// </summary>
    [SerializeField]
    protected TurnMode _turnMode = TurnMode.Snap;
    public TurnMode turnMode
    {
        get => _turnMode;
        set
        {
            if(_turnMode != value)
            {
                _turnMode = value;
                UpdateTurnMode();
            }
        }
    }

    public UnityEvent ChangedMovementMode = new UnityEvent();
    public UnityEvent ChangedTurnMode = new UnityEvent();

    

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
        } 
        else {
            moveProvider.enabled = false;
        }
        if(ChangedMovementMode != null)
        {
            ChangedMovementMode.Invoke();
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
        if(ChangedTurnMode != null)
        {
            ChangedTurnMode.Invoke();
        }
    }

    public void SetMoveMode(Int32 val)
    {
        movementMode = (MovementMode)val;
    }

    public void SetTurnMode(Int32 val)
    {
        turnMode = (TurnMode)val;
    }
}
