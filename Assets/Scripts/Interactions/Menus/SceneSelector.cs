using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class SceneSelector : Selector<FieldScene>
{
    public SceneEvent ChangeScene = new SceneEvent();

    public TextMeshProUGUI nameDisplay;
    public TextMeshProUGUI descriptionDisplay;





    protected override void Start()
    {
        base.Start();

        if(UIAppearEvent != null) {
            UIAppearEvent.Invoke(canvas);
        }
    }



    protected override void ChangeSelection()
    {
        if (current >= available.Length || available[current] == null) {
            Debug.LogError("Empty available array or array entry detected.");
            return;
        }

        if (ChangeScene != null) {
            ChangeScene.Invoke(available[current]);
        }
        nameDisplay.SetText(available[current].name);
        descriptionDisplay.SetText(available[current].SceneDescription);
    }



    protected override void ReactToPlayer()
    {
        // Implement me (and also call me from somewhere) &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        //UIAppearEvent.Invoke(canvas);
        //UIDisppearEvent.Invoke(canvas);
    }
}

[System.Serializable]
public class SceneEvent : UnityEvent<FieldScene> { }