using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    /// <summary>
    /// The object to be facing.
    /// </summary>
    public Transform playerEyes; 
    /// <summary> 
    /// The canvas with all the relevant information.
    /// </summary>
    public Canvas canvas;
    /// <summary>
    /// The distance from the player at which the UI disappears
    /// </summary>
    [Min(1f)]
    public float visibleDistance;

    public delegate bool UIUpdated(Canvas canvas);
    /// <summary>
    /// A delegate to inform the player that the UI has appeared.
    /// </summary>
    public UIUpdated UIAppear = new UIUpdated(canvas => true);
    /// <summary>
    /// A delegate to inform the player that the UI has disappeared.
    /// </summary>
    public UIUpdated UIDisappear = new UIUpdated(canvas => true);



    private void Update()
    {
        ReactToPlayer();
    }



    /// <summary>
    /// Closes the display if the player is far enough away. 
    /// </summary>
    protected virtual void ReactToPlayer()
    {
        // Measure the distance to the player
        Vector3 displacement = transform.position - playerEyes.position;
        Vector3 planeDistance = new Vector3(displacement.x, 0, displacement.z);

        // Close the display if the player is far away. 
        if (planeDistance.magnitude <= visibleDistance)
        {
            if (!canvas.enabled)
            {
                canvas.enabled = true;
                UIAppear(canvas);
            }
        }
        else
        {
            if (canvas.enabled)
            {
                canvas.enabled = false;
                UIDisappear(canvas);
            }
        }
    }
}
