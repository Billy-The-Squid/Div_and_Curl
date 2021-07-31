using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Elevator : MonoBehaviour
{
    public float minHeight;
    public float maxHeight;
    protected MovementManager.MovementMode currentMoveMode;
    public Transform playerEyes;
    public float speed;
    public float radius;
    protected bool inTransit;
    protected XRRig xrRig;
    protected Transform rigParent;


    // Start is called before the first frame update
    void Start()
    {
        inTransit = false;
        xrRig = FindObjectOfType<XRRig>();
        CheckMovementMode();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentMoveMode == MovementManager.MovementMode.Smooth)
        {
            float planeDistance = FieldSelector.planeDist(playerEyes.position, transform.position).magnitude;
            float heightDiff = playerEyes.position.y - transform.position.y;

            if (planeDistance > radius || heightDiff <= 0)
            {
                if(transform.position.y > minHeight)
                {
                    StartCoroutine(Lower());
                }
            }
            else if(transform.position.y < maxHeight)
            {
                StartCoroutine(Raise());
            }
        }
        else
        {
            StartCoroutine(Raise());
        }
    }

    protected IEnumerator Raise()
    {
        if (!inTransit)
        {
            inTransit = true;
            if(currentMoveMode == MovementManager.MovementMode.Smooth)
            {
                rigParent = xrRig.transform.parent;
                xrRig.transform.parent = transform;
            }
            if (transform.position.y < maxHeight)
            {
                transform.position += speed * Time.deltaTime * Vector3.up;
                yield return null;
            }
            if(currentMoveMode == MovementManager.MovementMode.Smooth)
            {
                xrRig.transform.parent = rigParent;
            }
            inTransit = false;
        }
    }

    protected IEnumerator Lower()
    {
        if (!inTransit)
        {
            inTransit = true;
            if (transform.position.y > minHeight)
            {
                transform.position -= speed * Time.deltaTime * Vector3.up;
                yield return null;
            }
            inTransit = false;
        }
    }

    public void CheckMovementMode()
    {
        currentMoveMode = FindObjectOfType<MovementManager>().movementMode;
    }
}
