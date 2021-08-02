using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Grabbable))]
public class GrabbableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
    }
}
