using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Credits : MonoBehaviour
{
    public RectTransform content;
    public RectTransform mask;
    public float speed = 1;

    public UnityEvent creditsFinished;

    protected IEnumerator RollCredits()
    {
        while(content.transform.localPosition.y < content.rect.height + 0.5f * mask.rect.height)
        {
            content.transform.localPosition += speed * Time.deltaTime * Vector3.up;
            yield return null;
        }
        yield return new WaitForSeconds(2);
        creditsFinished.Invoke();
    }

    public void StartCredits()
    {
        content.transform.localPosition = new Vector3(0, 0.25f * mask.rect.height, 0);

        StartCoroutine(RollCredits());
    }
}
