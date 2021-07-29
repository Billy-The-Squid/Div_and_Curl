using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class SceneSelector : Selector<FieldScene>
{
    /// <summary> 
    /// The canvas with all the relevant information.
    /// </summary>
    public Canvas canvas;
    ///// <summary>
    ///// The background for the menu.
    ///// </summary>
    //public Collider background;

    public SceneEvent ChangeScene = new SceneEvent();

    public TextMeshProUGUI nameDisplay;
    public TextMeshProUGUI descriptionDisplay;





    protected override void Start()
    {
        base.Start();
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
        nameDisplay.SetText(available[current].sceneName);
        descriptionDisplay.SetText(available[current].sceneDescription);
    }


    public void ReloadScene()
    {
        ChangeSelection();
    }


    //protected void ReactToPlayer()
    //{
    //    // Implement me (and also call me from somewhere) &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    //    //UIAppearEvent.Invoke(canvas);
    //    //UIDisppearEvent.Invoke(canvas);
    //}
}

[System.Serializable]
public class SceneEvent : UnityEvent<FieldScene> { }