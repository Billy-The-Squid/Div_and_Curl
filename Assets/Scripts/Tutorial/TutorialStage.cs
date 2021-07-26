using UnityEngine;

public abstract class TutorialStage : MonoBehaviour
{
    public GameObject canvas;

    public abstract void BeginStage();

    public abstract void EndStage();
}
