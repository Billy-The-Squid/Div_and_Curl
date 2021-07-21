using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFieldScene", menuName = "Field Scene")]
public class FieldScene : ScriptableObject
{
    public string sceneName;
    [TextArea(minLines:1, maxLines:5)]
    public string sceneDescription;
    public DetectorData[] detectorArray;
    public FieldData[] fieldArray;
    public bool limitDetectors;
}
