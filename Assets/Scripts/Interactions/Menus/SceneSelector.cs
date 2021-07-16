using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SceneSelector : MonoBehaviour
{
    public Canvas canvas;

    protected int _currentGlobalScene;
    protected int currentGlobalScene
    {
        get => _currentGlobalScene;
        set
        {
            int val = value % scenesAvailable.Length;
            while(val < 0)
            {
                val += scenesAvailable.Length;
            }
            if(val != _currentGlobalScene)
            {
                _currentGlobalScene = val;
                ChangeScene.Invoke(scenesAvailable[val]);
            }
        }
    }

    public FieldScene[] scenesAvailable;

    public SceneEvent ChangeScene = new SceneEvent();

    public UIEvent DeleteMe = new UIEvent();





    // Start is called before the first frame update
    void Start()
    {// Send out the signal about the new scene.
        // Some sort of test for nulls in the scene being loaded %%%%%%%%%%%%%%%%%%%%%%%%%%%%
        if (ChangeScene != null) { ChangeScene.Invoke(scenesAvailable[0]); }

        DeleteMe.Invoke(canvas);
    }

    public void NextScene()
    {
        currentGlobalScene += 1;
    }

    public void PrevScene()
    {
        currentGlobalScene -= 1;
    }
}

[System.Serializable]
public class SceneEvent : UnityEvent<FieldScene> { }

[System.Serializable]
public struct FieldScene
{
    public string sceneName;
    public List<int> detectorList;
    public List<VectorField.FieldType> fieldList;
    public bool limitDetectors;
}