using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tutorial : MonoBehaviour
{
    public GameObject[] setDisabled;
    protected bool[] wasEnabled;

    public Canvas mainCanvas;
    public GameObject background;
    public Transform tutorialPivot;
    public TutorialStage[] frames;
    protected int _currentFrameIndex;
    public int currentFrameIndex
    {
        get => _currentFrameIndex;
        set
        {
            if(_currentFrameIndex != value)
            {
                frames[_currentFrameIndex].EndStage();
                frames[value].BeginStage();
                _currentFrameIndex = value;
                // Adjust background size &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            }
        }
    }

    protected DetectorSelector detectorSelector;
    protected FieldSelector fieldSelector;

    public UIEvent UIAppearEvent = new UIEvent();
    public UIEvent UIDisappearEvent = new UIEvent();

    public GameObject playerEyes;




    // Start is called before the first frame update
    void Start()
    {
        if(setDisabled != null && setDisabled.Length != 0)
        {
            wasEnabled = new bool[setDisabled.Length];
        }
        if(detectorSelector == null)
        {
            detectorSelector = FindObjectOfType<DetectorSelector>();
        }
        if(fieldSelector == null)
        {
            fieldSelector = FindObjectOfType<FieldSelector>();
        }

        StartCoroutine(BeginTutorial());
    }

    
    /// <summary>
    /// A wrapper function, mostly for inspector use.
    /// </summary>
    /// <param name="i"></param>
    public void GoToStage(int i)
    {
        currentFrameIndex = i;
        // Adjust background size &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&

    }

    public IEnumerator BeginTutorial()
    {
        yield return new WaitForSeconds(0.05f);

        // Turn stuff off
        for(int i = 0; i < setDisabled.Length; i++)
        {
            GameObject thing = setDisabled[i];
            wasEnabled[i] = thing.activeSelf;
            thing.SetActive(false);
        }
        detectorSelector.Sleep();
        fieldSelector.Sleep();

        // Turn on the tutorial
        mainCanvas.enabled = true;
        background.SetActive(true);
        GoToStage(0);
        frames[currentFrameIndex].BeginStage();
        UIAppearEvent.Invoke(mainCanvas);

        tutorialPivot.position = playerEyes.transform.position;
        tutorialPivot.forward = new Vector3(playerEyes.transform.forward.x, 0, playerEyes.transform.forward.z).normalized;

        // Set positions
        // Set modes
        // Prompt scene reload (prior to sleep mode, to clear out the objects in the scene)
    }

    public void EndTutorial()
    {
        frames[currentFrameIndex].EndStage();
        mainCanvas.enabled = false;
        background.SetActive(false);
        for(int i = 0; i < setDisabled.Length; i++)
        {
            setDisabled[i].SetActive(wasEnabled[i]);
        }
        UIDisappearEvent.Invoke(mainCanvas);

        // Bind this to a scene change!
    }
}
