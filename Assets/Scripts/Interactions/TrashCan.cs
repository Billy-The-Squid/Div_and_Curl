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
        Destroy(socket.selectTarget.gameObject);
    }
}
