using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VectorField))]
public class ZoneManager : MonoBehaviour
{
    [SerializeField]
    Collider zone;

    // Start is called before the first frame update
    void Start()
    {
        if(zone == null)
        {
            zone = this.GetComponent<Collider>();
            if(zone == null)
            {
                Debug.LogWarning("Could not find field zone collider");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            //Debug.Log("Detected collider");
            other.GetComponent<FieldDetector>().EnteredField(this.GetComponent<VectorField>());
        }
        catch (System.NullReferenceException)
        {
            ;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        try
        {
            //Debug.Log("Detected collider");
            other.GetComponent<FieldDetector>().ExitedField(this.GetComponent<VectorField>());
        }
        catch (System.NullReferenceException)
        {
            ;
        }
    }
}
