using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewDetectorData", menuName = "Detector Data")]
public class DetectorData : ScriptableObject
{
    public new string name;
    [TextArea(1, 20)]
    public string description;
    public Sprite equation;
    [TextArea(1, 20)]
    public string equationDescription;
    [TextArea(1, 20)]
    public string detector;
    public Grabbable detectorPrefab;
}
