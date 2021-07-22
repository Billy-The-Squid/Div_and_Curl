using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DetectorSelector : Selector<DetectorData>
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
    /// The background for the menu.
    /// </summary>
    public Collider background;
    /// <summary>
    /// The distance from the player at which the UI disappears
    /// </summary>
    [Min(1f)]
    public float visibleDistance;

    public UIEvent UIAppearEvent = new UIEvent();
    public UIEvent UIDisppearEvent = new UIEvent();

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

    public Dictionary<DetectorData, List<GameObject>> objectDict { get; protected set; }
    /// <summary>
    /// If the object sitting on the pedestal right now is the first one of its type to be created, this should be true. 
    /// </summary>
    public Dictionary<DetectorData, bool> isFirst { get; protected set; }
    // Should become false if object is moved at all. 

    public Color cantPullColor = Color.red;

    public RectTransform nextButton;
    public RectTransform previousButton;





    protected override void Start()
    {
        isFirst = new Dictionary<DetectorData, bool>();
        base.Start();
    }



    private void Update()
    {
        ReactToPlayer();
        if(instantiated == null) {
            ChangeSelection();
        }
        else if ((instantiated.transform.position - spawnPoint.position).magnitude > respawnDistance) {
            if(isFirst[available[current]]) {
                isFirst[available[current]] = false;
            }
            ChangeSelection();
        }

        //Debug.Log("Is first?" + isFirst[available[current]]);
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

        // Check isFirst
        if(!isFirst.ContainsKey(available[current])) {
            isFirst.Add(available[current], true);
        }

        // Do away with the old
        if(instantiated != null && ((instantiated.transform.position - spawnPoint.position).magnitude < respawnDistance)) {
            //Destroy(instantiated.gameObject);
            GetRidOf(available[current], instantiated.gameObject);
        }
        // And in with the new
        MakeANew(available[current]);

        // Update the displays.
        detectorNameDisplay.SetText(available[current].name);
        detectorDescriptionDisplay.SetText(available[current].description);
    }

    /// <summary>
    /// Neatly disposes of an instantiated object.
    /// </summary>
    /// <param name="data">The <see cref="DetectorData"/> type holding the prefab to be discarded.</param>
    /// <param name="obj">The object to be discarded.</param>
    protected void GetRidOf(DetectorData data, GameObject obj)
    {
        if (objectDict.ContainsKey(data) && objectDict[data] != null)
        {
            objectDict[data].Remove(obj);
        }
        Destroy(obj);
    }

    /// <summary>
    /// Instantiates a new object.
    /// </summary>
    /// <param name="detectorData">The <see cref="DetectorData"/> type holding the prefab to be instantiated.</param>
    protected void MakeANew(DetectorData detectorData)
    {
        //Debug.Log("Making a new " + detectorData.name);
        instantiated = Instantiate(detectorData.detectorPrefab);
        instantiated.transform.parent = spawnPoint;
        instantiated.transform.localPosition = new Vector3(0, 0, 0);

        if(objectDict != null)
        {
            if(!objectDict.ContainsKey(detectorData))
            {
                objectDict.Add(detectorData, new List<GameObject>());
            }

            objectDict[detectorData].Add(instantiated.gameObject);
        }
        else
        {
            Debug.LogError("Couldn't find the object dictionary");
        }
    }

    /// <summary>
    /// Called whenever <see cref="available"/> changes. 
    /// </summary>
    protected override void ChangeAvailable()
    {
        if(objectDict != null) {
            // Delete everything in the old set
            foreach (List<GameObject> list in objectDict.Values) {
                foreach(GameObject obj in list) {
                    if(obj != null) {
                        if (obj.GetComponent<Grabbable>() != null) {
                            obj.GetComponent<Grabbable>().enabled = false;
                        }
                        StartCoroutine(TrashCan.DestroyInteractable(obj));
                    }
                }
            }

            objectDict.Clear();
        }

        objectDict = new Dictionary<DetectorData, List<GameObject>>();

        base.ChangeAvailable();

        nextButton.gameObject.SetActive(HasNext());
        previousButton.gameObject.SetActive(HasPrevious());
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
    protected void ReactToPlayer()
    {
        // Rotate to face the player
        Vector3 displacement = transform.position - playerEyes.position;
        Vector3 planeDistance = new Vector3(displacement.x, 0, displacement.z);
        transform.forward = planeDistance.normalized;

        // Close the display if the player is far away. 
        if (planeDistance.magnitude <= visibleDistance)
        {
            if (!canvas.enabled)
            {
                canvas.enabled = true;
                UIAppearEvent.Invoke(canvas);
                if (background != null)
                {
                    background.enabled = true;
                }
            }
        }
        else
        {
            if (canvas.enabled)
            {
                canvas.enabled = false;
                UIDisppearEvent.Invoke(canvas);
                if (background != null)
                {
                    background.enabled = false;
                }
            }
        }
    }
}

