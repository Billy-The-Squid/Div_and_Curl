using System;
using UnityEngine;
using TMPro;


public class Settings : MonoBehaviour
{
    [Header("Displays")]
    public TMP_Dropdown handMode;
    public TMP_Dropdown moveMode;
    public TMP_Dropdown turnMode;

    public TextMeshProUGUI moveSpeedDisplay;
    public TextMeshProUGUI turnSpeedDisplay;
    public TextMeshProUGUI turnAngleDisplay;

    public RectTransform moveSpeedPanel;
    public RectTransform turnSpeedPanel;
    public RectTransform turnAnglePanel;

    [Header("References")]
    public MovementManager moveManager;

    public HandManager leftHand;
    public HandManager rightHand;

    [Header("Parameters")]
    public float minMoveSpeed;
    public float maxMoveSpeed;
    public float minTurnSpeed;
    public float maxTurnSpeed;
    public float minTurnAngle;
    public float maxTurnAngle;

    //public float moveSpeedIncrement;
    //public float turnSpeedIncrement;
    //public float turnAngleIncrement;

    protected float moveSpeed
    {
        get => moveManager.moveProvider.moveSpeed;
        set
        {
            moveManager.moveProvider.moveSpeed = value;
            moveSpeedDisplay.SetText(String.Format("{0:0.0}", value));
        }
    }
    protected float turnSpeed
    {
        get => moveManager.smoothTurnProvider.turnSpeed;
        set
        {
            moveManager.smoothTurnProvider.turnSpeed = value;
            turnSpeedDisplay.SetText(String.Format("{0:0}", value));
        }
    }
    protected float turnAngle
    {
        get => moveManager.snapTurnProvider.turnAmount;
        set
        {
            moveManager.snapTurnProvider.turnAmount = value;
            turnAngleDisplay.SetText("{0:0}", value);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        handMode.value = (int)leftHand.mode;
        moveMode.value = (int)moveManager.movementMode;
        turnMode.value = (int)turnMode.value;

        moveSpeedDisplay.SetText(String.Format("{0:0.0}", moveSpeed));
        turnSpeedDisplay.SetText(String.Format("{0:0}", turnSpeed));
        turnAngleDisplay.SetText(String.Format("{0:0}", turnAngle));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeMoveSpeed(float change)
    {
        if(moveSpeed + change <= maxMoveSpeed && moveSpeed + change >= minMoveSpeed)
        {
            moveSpeed += change;
        }
    }

    public void ChangeTurnSpeed(float change)
    {
        if(turnSpeed + change >= minTurnSpeed && turnSpeed + change <= maxTurnSpeed)
        {
            turnSpeed += change;
        }
    }

    public void ChangeTurnAngle(float change)
    {
        if(turnAngle + change >= minTurnAngle && turnAngle + change <= maxTurnAngle)
        {
            turnAngle += change;
        }
    }

    public void handModeChanged(int newMode)
    {
        handMode.value = newMode;
    }

    public void MoveModeChanged()
    {
        moveMode.value = (int)moveManager.movementMode;
        moveSpeedPanel.gameObject.SetActive(moveMode.value == (int)MovementManager.MovementMode.Smooth);
        moveSpeedDisplay.SetText(String.Format("{0:0.0}", moveSpeed));
    }

    public void TurnModeChanged()
    {
        turnMode.value = (int)moveManager.turnMode;
        turnSpeedPanel.gameObject.SetActive(moveManager.turnMode == MovementManager.TurnMode.Smooth);
        turnAnglePanel.gameObject.SetActive(moveManager.turnMode == MovementManager.TurnMode.Snap);
        turnSpeedDisplay.SetText(String.Format("{0:0}", turnSpeed));
        turnAngleDisplay.SetText(String.Format("{0:0}", turnAngle));
    }
}
