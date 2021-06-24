using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Caller : MonoBehaviour
{
    [SerializeField]
    FieldZone zone;

    // Update is called once per frame
    void Update()
    {
        zone.SetPositions();
    }
}
