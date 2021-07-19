using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

//[RequireComponent(typeof(XRSocketInteractor))]
public class TrashCan : XRSocketInteractor
{
    [SerializeField]
    public XRSocketInteractor socket;

    [SerializeField]
    public GameObject lid;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //if(socket == null) {
        //    socket = GetComponent<XRSocketInteractor>();
        //}
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
        //// Identify the object
        //GameObject toBeDestroyed = socket.selectTarget.gameObject;

        //// Pause socket interaction
        //socket.socketActive = false;
        ////socket.enabled = false;
        ////socket.allowSelect = false;

        //// Get rid of the object
        ////toBeDestroyed.SetActive(false);
        ////Debug.LogWarning("Set " + toBeDestroyed.name + " false");

        //StartCoroutine(DestroySocketed(toBeDestroyed));
        //Debug.Log("Started coroutine");
    }

    protected IEnumerator DestroySocketed(GameObject obj)
    {
        yield return new WaitForEndOfFrame();

        //Destroy(obj);
        obj.SetActive(false);

        yield return null; // Wait for next frame?

        Destroy(obj);

        //socket.socketActive = true;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        SelectExitEventArgs exitArgs = new SelectExitEventArgs();
        exitArgs.interactable = args.interactable;
        exitArgs.interactor = args.interactor;
        OnSelectExiting(exitArgs);
        StartCoroutine(DestroySocketed(args.interactable.gameObject));
    }
}
