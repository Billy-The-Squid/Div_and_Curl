using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable), typeof(Outline))]
public abstract class Grabbable : MonoBehaviour
{
    /// <summary>
    /// The name displayed in the selector menu
    /// </summary>
    public abstract string displayName { get; set; } // Should be static? Or something?

    /// <summary>
    /// The description to be displayed in the selector menu.
    /// </summary>
    public abstract string displayDescription { get; set; }

    /// <summary>
    /// The index used to identify the grabbable by the detector menu.
    /// </summary>
    public abstract int menuIndex { get; set; }
}
