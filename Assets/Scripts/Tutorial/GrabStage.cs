using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabStage : TutorialStage
{
    public GameObject continueButton;
    public GameObject skipText;
    public GameObject skipButton;

    public GameObject thingPrefab;
    public Vector3 spawnPoint;
    protected Grabbable instance;

    public override void BeginStage()
    {
        canvas.SetActive(true);

        instance = (Instantiate(thingPrefab)).GetComponent<Grabbable>();
        instance.transform.position = spawnPoint;
        instance.tag = "Tutorial object";

        continueButton.SetActive(false);
        skipText.SetActive(true);
        skipButton.SetActive(true);
    }

    public override void EndStage()
    {
        canvas.SetActive(false);
    }

    private void Update()
    {
        if(!canvas.activeSelf) { return; }

        if(instance.isSelected && !continueButton.activeSelf)
        {
            continueButton.SetActive(true);
            skipText.SetActive(false);
            skipButton.SetActive(false);
        }
    }
}
