using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MinMaxColor))]
public class MinMaxEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Recalculate magnitudes", EditorStyles.miniButton))
        {
            ((MinMaxColor)this.target).RecalculateMaxMagnitude();
        }

        base.OnInspectorGUI();
    }
}
