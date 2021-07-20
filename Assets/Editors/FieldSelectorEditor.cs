using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldSelector))]
public class FieldSelectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Next Field", EditorStyles.miniButton))
        {
            ((FieldSelector)this.target).Next();
        }

        if (GUILayout.Button("Previous Field", EditorStyles.miniButton))
        {
            ((FieldSelector)this.target).Previous();
        }
    }
}
