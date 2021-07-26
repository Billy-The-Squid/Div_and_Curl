using UnityEngine;
using UnityEngine.Events;

public class TeleportStage : TutorialStage
{
    public GameObject teleportAnchor;
    public UnityEvent teleported = new UnityEvent();
    public GameObject playerEyes;
    public Transform tutorialPivot;

    public override void BeginStage()
    {
        canvas.SetActive(true);
    }

    public override void EndStage()
    {
        canvas.SetActive(false);
    }

    private void Update()
    {
        if(!canvas.activeSelf) { return; }

        Vector3 planeDist = playerEyes.transform.position - teleportAnchor.transform.position;
        planeDist.y = 0;

        if (planeDist.magnitude < 0.5f)
        {
            tutorialPivot.position = playerEyes.transform.position;
            tutorialPivot.forward = new Vector3(playerEyes.transform.forward.x, 0, playerEyes.transform.forward.z).normalized;
            teleported.Invoke();
        }
    }
}
