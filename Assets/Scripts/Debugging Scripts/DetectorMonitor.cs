using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorMonitor : MonoBehaviour
{
    public FieldDetector[] detectors;
    private float[] maxima;

    private void Start()
    {
        maxima = new float[detectors.Length];
        for(int i = 0; i < detectors.Length; i++)
        {
            maxima[i] = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //foreach(FieldDetector detector in detectors)
        for(int i = 0; i < detectors.Length; i++)
        {
            //Debug.Log(detectors[i].quantityName + ": " + detectors[i].detectorOutput);
            //if(Mathf.Abs(detectors[i].detectorOutput) > maxima[i])
            //{
            //    maxima[i] = Mathf.Abs(detectors[i].detectorOutput);
            //}
        }
    }

    private void OnDestroy()
    {
        for(int i = 0; i < detectors.Length; i++)
        {
            //Debug.Log("Max " + detectors[i].quantityName + ": " + maxima[i]);
        }
    }
}
