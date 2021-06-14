using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resizable : MonoBehaviour
{
    [SerializeField]
    private float minRad = 0.05f, maxRad = 1f;

    [SerializeField] // Find some way to clamp here
    public float radius;

    private void Awake()
    {
        radius = Mathf.Clamp(radius, minRad, maxRad);
        transform.localScale = Vector3.one * radius;
    }

    public void SizeUp()
    {
        radius = Mathf.Clamp(radius + 0.1f, minRad, maxRad);
        transform.localScale = Vector3.one * radius;
    }

    public void SizeDown()
    {
        radius = Mathf.Clamp(radius - 0.1f, minRad, maxRad);
        transform.localScale = Vector3.one * radius;
    }
}
