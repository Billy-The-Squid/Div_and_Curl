using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


// Needs documentation
public class ForcePull : MonoBehaviour
{
    /// <summary>
    /// The object that either will be pulled or is being pulled.
    /// </summary>
    public XRGrabInteractable nearestGrabbable { get; set; }

    /// <summary>
    /// Is this hand currently pulling?
    /// </summary>
    public bool pulling;

    /// <summary>
    /// The input actions asset that the grab actions will be read from. 
    /// </summary>
    [SerializeField]
    InputActionAsset actionAsset;
    /// <summary>
    /// The grab action.
    /// </summary>
    protected InputAction grab;

    protected enum Hands { Right, Left }
    /// <summary>
    /// Which hand is this?
    /// </summary>
    [SerializeField]
    Hands hand;
    protected string[] handNames = { "Right", "Left" };

    /// <summary>
    /// The speed at which the grabbable is pulled towards you.
    /// </summary>
    [SerializeField]
    float pullSpeed;

    /// <summary>
    /// The XRDirect Interactor attached to this hand. 
    /// </summary>
    public XRDirectInteractor directInteractor;

    /// <summary>
    /// The transform towards which to pull the object.
    /// </summary>
    public Transform attachAnchorTransform;

    protected HandManager.GrabMode _grabMode;
    public HandManager.GrabMode grabMode
    {
        get => _grabMode;
        set
        {
            if(_grabMode != value)
            {
                togglePull = false;
                _grabMode = value;
            }
        }
    }

    /// <summary>
    /// When there is nothing grabbed, is flipped on each "Performed" callback. 
    /// </summary>
    protected bool togglePull = false;






    private void Start()
    {
        // Find the input action. 
        string handName = handNames[(int)hand];
        grab = actionAsset.FindActionMap("XRI " + handName + "Hand").FindAction("Select");
        // Flips togglePull, provided we're not holding anything, but we ARE aiming at something.
        grab.performed += (value) => togglePull = (!togglePull && directInteractor.selectTarget == null && nearestGrabbable != null);

        pulling = false;

        if (directInteractor == null)
        {
            directInteractor = GetComponent<XRDirectInteractor>();
        }
    }



    // Update is called once per frame
    void Update()
    {
        if(nearestGrabbable != null)
        {
            if(grabMode == HandManager.GrabMode.Hold && grab.phase != InputActionPhase.Waiting)
            {
                Pull();
            }
            else if (grabMode == HandManager.GrabMode.Toggle && togglePull)
            {
                Pull();
            }
            else
            {
                pulling = false;
            }
        } else
        {
            pulling = false;
        }

        void Pull()
        {
            if (nearestGrabbable.isSelected && nearestGrabbable.selectingInteractor is XRSocketInteractor)
            {
                StartCoroutine(StealFromSocket((XRSocketInteractor)nearestGrabbable.selectingInteractor));
            }
            else
            {
                nearestGrabbable.GetComponent<Rigidbody>().MovePosition(Vector3.MoveTowards(
                nearestGrabbable.transform.position, attachAnchorTransform.position, pullSpeed * Time.deltaTime));
            }
            pulling = directInteractor.selectTarget == null;
        }
    }



    /// <summary>
    /// When trying to grab an object from a socket, temporarily disables the socket's grab interaction. 
    /// </summary>
    /// <param name="socket">The socket to disable</param>
    /// <returns></returns>
    protected IEnumerator StealFromSocket(XRSocketInteractor socket)
    {
        socket.allowSelect = false;

        yield return new WaitForSeconds(0.5f);

        socket.allowSelect = true;
    }
}
