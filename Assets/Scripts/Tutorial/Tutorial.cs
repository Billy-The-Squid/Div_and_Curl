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




    // Start is called before the first frame update
    void Start()
    {
        if(setDisabled != null && setDisabled.Length != 0)
        {
            wasEnabled = new bool[setDisabled.Length];
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

        mainCanvas.enabled = true;
        background.SetActive(true);
        for(int i = 0; i < setDisabled.Length; i++)
        {
            GameObject thing = setDisabled[i];
            wasEnabled[i] = thing.activeSelf;
            thing.SetActive(false);
        }

        GoToStage(0);
        frames[currentFrameIndex].BeginStage();

        // Set positions
        // Set modes
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

        // Bind this to a scene change!
    }
}
