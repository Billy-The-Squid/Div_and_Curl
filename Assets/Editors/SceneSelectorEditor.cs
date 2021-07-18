using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneSelector))]
public class SceneSelectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Next Scene", EditorStyles.miniButton))
        {
            ((SceneSelector)this.target).Next();
        }

        if (GUILayout.Button("Previous Scene", EditorStyles.miniButton))
        {
            ((SceneSelector)this.target).Previous();
        }
    }
}
