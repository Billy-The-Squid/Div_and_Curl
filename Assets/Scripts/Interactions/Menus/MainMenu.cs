using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    public InputActionAsset actionAsset;
    protected InputActionMap map;
    protected InputAction action;

    /// <summary>
    /// The object to be facing.
    /// </summary>
    public Transform playerEyes;
    /// <summary> 
    /// The canvas with all the relevant information.
    /// </summary>
    public Canvas canvas;
    /// <summary>
    /// The background for the menu.
    /// </summary>
    public Collider background;

    public UIEvent UIAppearEvent = new UIEvent();
    public UIEvent UIDisppearEvent = new UIEvent();

    public Canvas[] subscreens;
    protected int _currentSubscreen;
    public int currentSubscreen
    {
        get => _currentSubscreen;
        set
        {
            if(_currentSubscreen != value)
            {
                if (value < subscreens.Length)
                {
                    if (subscreens[value] != null)
                    {
                        subscreens[_currentSubscreen].enabled = false;
                        subscreens[value].enabled = true;
                        _currentSubscreen = value;
                    }
                }
            }
        }
    }

    [Tooltip("The index of the screen in the subscreens array to start with on launch.")]
    public int startingSubscreenIndex = 0;




    protected void Start()
    {
        StartCoroutine(OpeningScreen());

        FindInputAction();
    }

    protected void Update()
    {
        if(action.triggered)
        {
            MenuButtonPressed();
        }
    }

    protected IEnumerator OpeningScreen()
    {
        yield return new WaitForSeconds(0.1f);

        BringUpMenu(startingSubscreenIndex);
    }

    protected void FindInputAction()
    {
        if (actionAsset == null) {
            throw new System.NullReferenceException("Please actually give me an input action asset");
        }

        map = actionAsset.FindActionMap("XRI LeftHand");
        if (map == null) {
            throw new System.NullReferenceException("Make sure you provided the correct map name.");
        }

        action = map.FindAction("Summon menu");
        if (action == null) {
            throw new System.NullReferenceException("Make sure you provided the correct action name");
        }
    }

    public void MenuButtonPressed()
    {
        //Debug.Log("Button was pressed");
        if(canvas.enabled) {
            DismissMenu();
        }
        else {
            BringUpMenu();
        }
    }

    public void BringUpMenu()
    {
        BringUpMenu(0);
    }

    public void BringUpMenu(int subscreenIndex)
    {
        transform.position = playerEyes.position;
        transform.forward = new Vector3(playerEyes.forward.x, 0, playerEyes.forward.z).normalized;
        //Debug.Log("Bringing up menu");

        canvas.enabled = true;
        background.gameObject.SetActive(true);
        foreach (Canvas subscreen in subscreens)
        {
            subscreen.enabled = false;
        }
        currentSubscreen = subscreenIndex; // Starting menu
        subscreens[subscreenIndex].enabled = true;
        if(UIAppearEvent != null)
        {
            UIAppearEvent.Invoke(canvas);
        }
    }

    public void DismissMenu()
    {
        //Debug.Log("Dismissing menu");
        canvas.enabled = false;
        background.gameObject.SetActive(false);
        if(UIDisppearEvent != null)
        {
            UIDisppearEvent.Invoke(canvas);
        }
    }

    //// Exclusively for UI interactions
    //public void ChangeSubscreen(int newSubscreenIndex)
    //{
    //    currentSubscreen = newSubscreenIndex;
    //}
}
