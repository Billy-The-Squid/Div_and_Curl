using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleColor : ColorScheme
{
    public Color color;

    /// <inheritdoc/>
    public override void ColorMaterial()
    {
        display.pointerMaterial.SetColor("_Color", color);
    }
}
