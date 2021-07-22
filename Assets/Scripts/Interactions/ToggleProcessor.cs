using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class ToggleProcessor : InputProcessor<float>
{
    public HandManager.Hand hand;
    protected bool resetToZero = false;
    protected HandManager handManager;

    public override float Process(float value, InputControl control)
    {
        Debug.LogWarning("Process called, with value " + value); 
        // Find the hand
        if (handManager == null)
        {
            HandManager[] handArray = Object.FindObjectsOfType<HandManager>();
            foreach (HandManager hnd in handArray)
            {
                if (hnd.hand == hand)
                {
                    handManager = hnd;
                }
            }
            if (handManager != null)
            {
                Debug.Log("Found hand: " + handManager.name);
            }
            else
            {
                Debug.Log("Did not find hand");
            }
        }

        // If we push the button
        if(value == 1)
        {
            // If we're already holding something, drop it 
            if(handManager.directInteractor.selectTarget != null || handManager.forcePuller.pulling)
            {
                Debug.Log("Processor thinks we're holding something and wants to try to drop it");
                return 0;
            }
            // Otherwise, try to grab.
            else
            {
                Debug.Log("Processor thinks we're trying to grab.");
                return 1;
            }
            //resetToZero = false;
        }
        // If we release the button
        else if (value == 0)
        {
            // If we're holding nothing, stop trying to grab. 
            //resetToZero = true;
            if (handManager.directInteractor.selectTarget == null && !handManager.forcePuller.pulling)
            {
                Debug.Log("Processor stops trying to grab");
                return 0;
            }
            // If we're holding something, keep up the good work. 
            else
            {
                Debug.Log("Processor thinks we're already holding something and should keep it up");
                return 1;
            }
        }
        else // If some dummy bound this to something other than a button. 
        {
            Debug.LogWarning("The Toggle process really shouldn't be bound to anything other than a button");
            return 0;
        }
    }


     
    #if UNITY_EDITOR
    static ToggleProcessor()
    {
        Initialize();
    }
    #endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        Debug.Log("Processor initialized");
        InputSystem.RegisterProcessor<ToggleProcessor>();
    }
}
