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
    //public enum Hand { Right, Left }
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
            if (handManager != null)
            {
                Debug.Log("Found hand: " + handManager.name);
            }
            else
            {
                Debug.Log("Did not find hand");
            }
        }

        {
            //if (context.phase == InputActionPhase.Waiting && context.ReadValue<float>() == 1)
            //{
            //    context.Started();
            //    context.PerformedAndStayStarted();
            //}

            //if(context.phase == InputActionPhase.Started) {
            //    if(context.ReadValue<float>() == 0)
            //    {
            //        resetToZero = true;
            //    }

            //    if(resetToZero && context.ReadValue<float>() == 1)
            //    {
            //        context.Canceled();
            //        context.Waiting();
            //    }
            //}
        }

        //Debug.Log("Called Process\nresetToZero: " + resetToZero + ", phase: " + context.phase);
        if(context.ReadValue<float>() == 1)// && resetToZero)
        {
            //Debug.Log("Got through")
            if(handManager.directInteractor.selectTarget != null || handManager.forcePuller.pulling)
            {
                context.Canceled();
                context.Waiting();
            }
            else
            {
                context.Started();
                context.PerformedAndStayPerformed();
            }
            //resetToZero = false;
        }
        else if (context.ReadValue<float>() == 0)
        {
            resetToZero = true;
            if(handManager.directInteractor.selectTarget == null && !handManager.forcePuller.pulling)
            {
                context.Canceled();
                context.Waiting();
            }
        }

        //Debug.LogWarning("Process not implemented");
    }

    public void Reset()
    {
        Debug.LogWarning("Reset not implemented");
    }


    //// Didn't work, but was pretty neat to try.
    //public Toggle()
    //{
    //    // Find the hand
    //    if (handManager == null)
    //    {
    //        HandManager[] handArray = Object.FindObjectsOfType<HandManager>();
    //        foreach (HandManager hnd in handArray)
    //        {
    //            if (hnd.hand == hand)
    //            {
    //                handManager = hnd;
    //            }
    //        }
    //        if (handManager != null)
    //        {
    //            Debug.Log("Found hand: " + handManager.name);
    //        }
    //        else
    //        {
    //            Debug.Log("Did not find hand");
    //        }
    //    }
    //}

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
