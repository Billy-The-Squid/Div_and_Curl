using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Watch : MonoBehaviour
{
    public UnityEvent WatchButton;
    public TextMeshProUGUI display;
    protected float lastPokedTime;
    public float debounceTime = 0.1f;

    private void Start()
    {
        lastPokedTime = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(Time.time > lastPokedTime + debounceTime)
        {
            if (other.CompareTag("Finger"))
            {
                WatchButton.Invoke();
            }
            lastPokedTime = Time.time;
        }
    }

    private void Update()
    {
        display.SetText(DateTime.Now.ToString("hh:mm"));
    }
}
