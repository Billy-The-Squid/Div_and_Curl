using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class Toggle : IInputInteraction
{
    public HandManager.Hand hand;
    protected bool resetToZero = false;
    protected HandManager handManager;


    public void Process(ref InputInteractionContext context)
    {
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
            //if (handManager != null)
            //{
            //    Debug.Log("Found hand: " + handManager.name);
            //}
            //else
            //{
            //    Debug.Log("Did not find hand");
            //}
        }

        //Debug.Log("Called Process\nresetToZero: " + resetToZero + ", phase: " + context.phase);
        // Do the actual processing.
        if(context.ReadValue<float>() == 1)// && resetToZero)
        {
            //Debug.Log("Got through")
            if(handManager.directInteractor.selectTarget != null || handManager.forcePuller.pulling)
            {
                Debug.Log("Interactor drops what it is holding");
                context.Canceled();
                context.Waiting();
            }
            else 
            {
                Debug.Log("Interactor tries to pick something up");
                context.Started();
                context.PerformedAndStayPerformed();
            }
            //resetToZero = false;
        }
        else if (context.ReadValue<float>() == 0)
        {
            //resetToZero = true;
            if(handManager.directInteractor.selectTarget == null && !handManager.forcePuller.pulling)
            {
                Debug.Log("Interactor thinks we can stop trying to grab.");
                context.Canceled();
                context.Waiting();
            }
        }
    }

    public void Reset()
    {
        Debug.LogWarning("Reset not implemented");
    }



    // These two are essential.
    static Toggle()
    {
        InputSystem.RegisterInteraction<Toggle>();
    }

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        // This will be called at runtime, causing the constructor to be called.
    }
}
