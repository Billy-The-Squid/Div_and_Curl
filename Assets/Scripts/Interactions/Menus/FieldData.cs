using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFieldData", menuName = "Field Data")]
public class FieldData : ScriptableObject
{
    public new string name;
    public Sprite description;
    public VectorField.FieldType field;
    public float rescaleFactor = 1;
}
