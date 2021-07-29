using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewDetectorData", menuName = "Detector Data")]
public class DetectorData : ScriptableObject
{
    public new string name;
    [TextArea(1, 5)]
    public string description;
    public Sprite equation;
    [TextArea(1, 5)]
    public string equationDescription;
    [TextArea(1, 5)]
    public string detector;
    public Grabbable detectorPrefab;
}
