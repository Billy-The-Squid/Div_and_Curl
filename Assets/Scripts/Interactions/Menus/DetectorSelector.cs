using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DetectorSelector : Selector<DetectorData>
{
    /// <summary>
    /// The last-instantiated detector.
    /// </summary>
    public Grabbable instantiated { get; protected set; }

    /// <summary>
    /// The transform at which to spawn in new detectors.
    /// </summary>
    public Transform spawnPoint;
    /// <summary>
    /// The distance from the spawn point at which a new object should be spawned.
    /// </summary>
    public float respawnDistance = 0.05f;

    /// <summary>
    /// The display text bearing the detector name. 
    /// </summary>
    public TextMeshProUGUI detectorNameDisplay;
    /// <summary>
    /// The display text bearing the detector description.
    /// </summary>
    public TextMeshProUGUI detectorDescriptionDisplay;





    protected override void Start()
    {
        base.Start();
    }



    private void Update()
    {
        ReactToPlayer();
        if(instantiated == null || (instantiated.transform.position - spawnPoint.position).magnitude > respawnDistance)
        {
            ChangeSelection();
        }
    }



    /// <summary>
    /// Puts a new detector on the display.
    /// </summary>
    protected override void ChangeSelection()
    {
        // Verify that we can do this.
        if(current >= available.Length || available[current] == null) {
            Debug.LogError("Empty available array or array entry detected.");
            return;
        }

        // Do away with the old
        if(instantiated != null && ((instantiated.transform.position - spawnPoint.position).magnitude < respawnDistance)) {
            Destroy(instantiated.gameObject);
        }
        // And in with the new
        instantiated = Instantiate(available[current].detectorPrefab);
        instantiated.transform.position = spawnPoint.position;
        instantiated.transform.parent = spawnPoint;

        // Update the displays.
        detectorNameDisplay.SetText(available[current].name);
        detectorDescriptionDisplay.SetText(available[current].description);
    }



    /// <summary>
    /// Loads the set of detectors (and first detector) of a new scene.
    /// </summary>
    /// <param name="scene"></param>
    public void LoadScene(FieldScene scene)
    {
        available = scene.detectorArray;
        current = 0;
    }



    /// <summary>
    /// Rotates to face the player and closes the display if the player is far enough away. 
    /// </summary>
    protected override void ReactToPlayer()
    {
        // Rotate to face the player
        Vector3 displacement = transform.position - playerEyes.position;
        Vector3 planeDistance = new Vector3(displacement.x, 0, displacement.z);
        transform.forward = planeDistance.normalized;

        base.ReactToPlayer();
    }
}
