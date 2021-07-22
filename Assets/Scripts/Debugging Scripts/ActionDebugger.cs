using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class ActionDebugger : MonoBehaviour
{
    public InputActionAsset actionAsset;
    public string actionMapName;
    public string actionName;

    protected InputActionMap map;
    protected InputAction action;

    // Start is called before the first frame update
    void Start()
    {
        if(actionAsset == null)
        {
            throw new System.NullReferenceException("Please actually give me an input action asset");
        }

        map = actionAsset.FindActionMap(actionMapName);
        if(map == null)
        {
            throw new System.NullReferenceException("Make sure you provided the correct map name.");
        }

        action = map.FindAction(actionName);
        if(action == null)
        {
            throw new System.NullReferenceException("Make sure you provided the correct action name");
        }

        action.started += ActionStarted;
        action.performed += ActionPerformed;
        action.canceled += ActionCancelled;
    }

    // Update is called once per frame
    void Update()
    {
        if(map != null)
        {
            //Debug.Log("Map enabled? " + map.enabled);
        }

        if(action != null)
        {
            DebugAction(action);
        }
    }

    public static void DebugAction(InputAction action)
    {
        //Debug.Log("Map (" + action.actionMap.name + ") Action (" + action.name + ")");
        //Debug.Log("Action enabled?" + action.enabled);
        //Debug.Log("Read value (float): " + action.ReadValue<float>());
        //Debug.Log("Read value (bool): " + action.ReadValue<bool>());
        //Debug.Log("Triggered? " + action.triggered);
        //Debug.Log("Phase: " + action.phase);
    }

    public void ActionStarted(InputAction.CallbackContext context)
    {
        Debug.Log("Action " + action.name + " started");
    }

    public void ActionPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Action " + action.name + " performed");
    }

    public void ActionCancelled(InputAction.CallbackContext context)
    {
        Debug.Log("Action " + action.name + " cancelled");
    }
}
