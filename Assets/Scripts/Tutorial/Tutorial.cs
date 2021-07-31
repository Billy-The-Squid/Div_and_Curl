using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Tutorial : MonoBehaviour
{
    public GameObject[] setDisabled;
    //protected bool[] wasEnabled;

    public Canvas mainCanvas;
    public GameObject background;
    public Transform tutorialPivot;
    public float threshholdDistance = 1f;

    public TutorialStage[] frames;
    protected int _currentFrameIndex;
    public int currentFrameIndex
    {
        get => _currentFrameIndex;
        set
        {
            if(_currentFrameIndex != value)
            {
                frames[_currentFrameIndex].EndStage();
                frames[value].BeginStage();
                _currentFrameIndex = value;
                // Adjust background size &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            }
        }
    }

    protected DetectorSelector detectorSelector;
    protected FieldSelector fieldSelector;
    protected MainMenu mainMenu;
    public GameObject playerEyes;
    public SceneSelector sceneSelector;

    public FieldScene tutorialScene { get; protected set; }
    public FieldScene introduction;
    public bool inTutorial { get; protected set; }

    public UIEvent UIAppearEvent = new UIEvent();
    public UIEvent UIDisappearEvent = new UIEvent();

    public Vector3 startLocation;
    public Vector3 startFacing;
    //public TeleportationProvider teleportationProvider;
    public XRRig xRRig;
    //public Transform playerEyes;




    // Start is called before the first frame update
    void Start()
    {
        //if(setDisabled != null && setDisabled.Length != 0)
        //{
        //    wasEnabled = new bool[setDisabled.Length];
        //}
        detectorSelector = FindObjectOfType<DetectorSelector>();
        fieldSelector = FindObjectOfType<FieldSelector>();
        mainMenu = FindObjectOfType<MainMenu>();
        sceneSelector = FindObjectOfType<SceneSelector>();

        tutorialScene = ScriptableObject.CreateInstance<FieldScene>();
        //tutorialScene = new FieldScene();
        tutorialScene.detectorArray = new DetectorData[1];
        tutorialScene.detectorArray[0] = null;
        tutorialScene.fieldArray = new FieldData[1];
        tutorialScene.fieldArray[0] = null;

        StartTutorial();
    }

    private void Update()
    {
        if(!inTutorial) { return; }

        // Has the player moved away? 
        Vector3 planeDistance = playerEyes.transform.position - tutorialPivot.position;
        planeDistance = new Vector3(planeDistance.x, 0, planeDistance.z);
        if(planeDistance.magnitude > threshholdDistance)
        {
            ResetPosition();
        }
    }

    protected void ResetPosition()
    {
        tutorialPivot.position = playerEyes.transform.position;
        tutorialPivot.forward = new Vector3(playerEyes.transform.forward.x, 0, playerEyes.transform.forward.z).normalized;
    }

    /// <summary>
    /// A wrapper function, mostly for inspector use.
    /// </summary>
    /// <param name="i"></param>
    public void GoToStage(int i)
    {
        currentFrameIndex = i;
        //background.transform.localScale.y = frames[currentFrameIndex].canvas.GetComponent<RectTransform>().
        // Adjust background size &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
    }

    public void StartTutorial()
    {
        sceneSelector.current = 0;

        StartCoroutine(Initiate());

        IEnumerator Initiate()
        {
            yield return new WaitForSeconds(0.05f);
            sceneSelector.ChangeScene.Invoke(tutorialScene);
            //StartCoroutine(BeginTutorial());
        }
    }

    
    // This is called when the tutorial scene loads, I think?
    public void BeginTutorial()
    {
        inTutorial = true;

        // Turn stuff off
        for (int i = 0; i < setDisabled.Length; i++)
        {
            GameObject thing = setDisabled[i];
            //wasEnabled[i] = thing.activeSelf;
            thing.SetActive(false);
        }
        //detectorSelector.Sleep();
        //fieldSelector.Sleep();
        //sceneSelector.ChangeScene.Invoke(tutorialScene);
        mainMenu.DismissMenu();

        // Turn on the tutorial
        mainCanvas.enabled = true;
        background.SetActive(true);
        GoToStage(0);
        frames[currentFrameIndex].BeginStage();
        UIAppearEvent.Invoke(mainCanvas);

        ResetPlayerPosition();

        ResetPosition();

        // Set positions
        // Set modes
        // Prompt scene reload (prior to sleep mode, to clear out the objects in the scene)
    }

    public void ResetPlayerPosition()
    {
        xRRig.MoveCameraToWorldLocation(startLocation + xRRig.cameraInRigSpaceHeight * Vector3.up);
        xRRig.MatchRigUpCameraForward(Vector3.up, startFacing);

        //TeleportRequest request = new TeleportRequest();
        //request.destinationPosition = startLocation;
        //request.destinationRotation = Quaternion.Euler(startFacing);
        //request.matchOrientation = MatchOrientation.TargetUp;
        //teleportationProvider.QueueTeleportRequest(request);
    }

    public void EndTutorial()
    {
        frames[currentFrameIndex].EndStage();
        mainCanvas.enabled = false;
        background.SetActive(false);
        for(int i = 0; i < setDisabled.Length; i++)
        {
            setDisabled[i].SetActive(true); // wasEnabled[i]);
        }
        UIDisappearEvent.Invoke(mainCanvas);

        foreach(GameObject thing in GameObject.FindGameObjectsWithTag("Tutorial object"))
        {
            Destroy(thing);
        }

        inTutorial = false;

        // Bind this to a scene change!
    }

    public void LoadScene(FieldScene scene)
    {
        if(scene == tutorialScene) { BeginTutorial(); }
        else 
        { 
            if(scene != introduction)
            {
                EndTutorial();
            }
        }
    }
}
