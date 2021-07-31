using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuStage : TutorialStage
{
    public int nextStageIndex;

    protected Tutorial tutorial;
    private void Start()
    {
        tutorial = FindObjectOfType<Tutorial>();
    }

    public override void BeginStage()
    {
        canvas.SetActive(true);
    }

    public override void EndStage()
    {
        canvas.SetActive(false);
    }

    public void LoadScene(FieldScene scene)
    {
        if(scene == tutorial.introduction)
        {
            tutorial.GoToStage(nextStageIndex);
        }
    }
}
