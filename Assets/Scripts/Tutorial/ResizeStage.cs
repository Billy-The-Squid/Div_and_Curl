using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeStage : TutorialStage
{
    protected GameObject obj;
    protected float currentScale;

    public GameObject continueButton;

    public override void BeginStage()
    {
        canvas.SetActive(true);

        obj = GameObject.FindGameObjectWithTag("Tutorial object");
        currentScale = obj.transform.localScale.x;

        continueButton.SetActive(false);
    }

    public override void EndStage()
    {
        canvas.SetActive(false);
    }

    private void Update()
    {
        if(!canvas.activeSelf) { return; }

        if(Mathf.Abs(obj.transform.localScale.x - currentScale) > 0.005f)
        {
            continueButton.SetActive(true);
        }
    }
}
