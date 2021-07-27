using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Watch : MonoBehaviour
{
    public UnityEvent WatchButton;
    public TextMeshProUGUI display;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Something poked the watch: " + other.name);
        if (other.CompareTag("Finger"))
        {
            WatchButton.Invoke();
            //Debug.Log("It was a finger");
        }
    }

    private void Update()
    {
        display.SetText(DateTime.Now.ToString("hh:mm"));
    }
}
