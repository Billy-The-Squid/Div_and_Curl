using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRSocketInteractor))]
public class TrashCan : MonoBehaviour
{
    [SerializeField]
    public XRSocketInteractor socket;

    [SerializeField]
    public GameObject lid;

    // Start is called before the first frame update
    void Start()
    {
        if(socket == null) {
            socket = GetComponent<XRSocketInteractor>();
        }
    }

    private void Update()
    {
        if(socket.selectTarget == null)
        {
            Debug.Log("No select target");
        }
        else
        {
            Debug.Log("Select target: " + socket.selectTarget.name);
        }
        Debug.Log("Socket active: " + socket.socketActive);
    }

    public void EnteredHover()
    {
        lid.GetComponent<MeshRenderer>().enabled = false;
    }

    public void ExitedHover()
    {
        lid.GetComponent<MeshRenderer>().enabled = true;
    }

    public void Selected()
    {
        // Identify the object
        GameObject toBeDestroyed = socket.selectTarget.gameObject;

        // Pause socket interaction
        socket.socketActive = false;
        //socket.enabled = false;
        //socket.allowSelect = false;

        // Get rid of the object
        //toBeDestroyed.SetActive(false);
        //Debug.LogWarning("Set " + toBeDestroyed.name + " false");

        StartCoroutine(DestroySocketed(toBeDestroyed));
        Debug.Log("Started coroutine");
    }

    protected IEnumerator DestroySocketed(GameObject obj)
    {
        yield return new WaitForEndOfFrame();

        Destroy(obj);

        yield return null; // Wait for next frame?

        socket.socketActive = true;
    }
}
