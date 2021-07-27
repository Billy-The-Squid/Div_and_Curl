using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashStage : TutorialStage
{
    protected GameObject obj;
    public GameObject trashCanPrefab;
    protected GameObject trashCanInstance;
    public GameObject continueButton;

    public override void BeginStage()
    {
        canvas.SetActive(true);

        obj = GameObject.FindGameObjectWithTag("Tutorial object");
        trashCanInstance = Instantiate(trashCanPrefab);
        trashCanInstance.SetActive(true);
        continueButton.SetActive(false);

        Vector3 pos = new Vector3(transform.position.x, 0.5f, transform.position.z);
        pos -= transform.forward * 0.1f;
        trashCanInstance.transform.position = pos;
    }

    public override void EndStage()
    {
        canvas.SetActive(false);

        Destroy(trashCanInstance);
    }

    private void Update()
    {
        if(!canvas.activeSelf) { return; }

        if(obj == null)
        {
            continueButton.SetActive(true);
        }
    }
}
