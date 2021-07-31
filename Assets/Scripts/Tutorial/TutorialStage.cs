using UnityEngine;

public class TutorialStage : MonoBehaviour
{
    public GameObject canvas;

    public virtual void BeginStage()
    {
        canvas.SetActive(true);
    }

    public virtual void EndStage()
    {
        canvas.SetActive(false);
    }
}
