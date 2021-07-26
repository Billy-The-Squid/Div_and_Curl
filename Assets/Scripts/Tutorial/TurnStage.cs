using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnStage : TutorialStage
{
    public override void BeginStage()
    {
        canvas.SetActive(true);
    }

    public override void EndStage()
    {
        canvas.SetActive(false);
    }
}
