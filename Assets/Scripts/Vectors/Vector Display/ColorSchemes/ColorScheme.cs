using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ColorScheme : MonoBehaviour
{
    /// <summary>
    /// The <see cref="VectorDisplay"/> being used to actually plot the vectors.
    /// </summary>
    public VectorDisplay display;
    // Used to access the shader and material both.

    /// <summary>
    /// The main function. Sets the necessary properties of the shader to color the material.
    /// </summary>
    public abstract void ColorMaterial();
}
