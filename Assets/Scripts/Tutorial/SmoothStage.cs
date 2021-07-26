using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothStage : TutorialStage
{
    public MovementManager movement;

    public override void BeginStage()
    {
        canvas.SetActive(true);

        movement.movementMode = MovementManager.MovementMode.Smooth;
    }

    public override void EndStage()
    {
        canvas.SetActive(false);
    }
}
