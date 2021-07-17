using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This is a short script to mediate the transitions between "scenes" without changing scenes. 
/// </summary>
public class SettingManager : MonoBehaviour
{
    public VectorField mainVectorField;

    public GameObject divDetector;
    public GameObject curlDetector;
    public GameObject fluxDetector;
    public GameObject loopIntegrator;
    public GameObject basicInteractor;

    protected GameObject currentDetector;

    public enum Scene 
    { 
        Interactions, 
        FieldInwards, 
        FieldSwirl, 
        FieldSquish, 
        FieldVortices, 

        DivDetector, 
        FluxDetector, 
        CurlDetector, 
        LoopIntegrator 
    }
    public Scene currentScene;
    protected Scene pastScene;

    static VectorField.FieldType[] fieldTypes =
    {
        VectorField.FieldType.Empty,
        VectorField.FieldType.Inwards,
        VectorField.FieldType.Swirl,
        VectorField.FieldType.Squish,
        VectorField.FieldType.Vortices,

        VectorField.FieldType.Squish,
        VectorField.FieldType.Squish,
        VectorField.FieldType.Vortices,
        VectorField.FieldType.Vortices
    };

    static GameObject[] detectors;

    protected bool refreshColors;

    public InputActionAsset actionAsset;
    protected InputAction next, previous;

    public Transform spawnPoint;













    // Start is called before the first frame update
    void Start()
    {
        //currentScene = 0;
        pastScene = currentScene + 1;
        mainVectorField.isDynamic = true;

        refreshColors = false;

        detectors = new GameObject[] {
            basicInteractor,
            null,
            null,
            null,
            null,

            divDetector,
            fluxDetector,
            curlDetector,
            loopIntegrator
        };

        actionAsset.FindActionMap("Scene transition").Enable();
        next = actionAsset.FindActionMap("Scene transition").FindAction("Next Scene");
        previous = actionAsset.FindActionMap("Scene transition").FindAction("Previous Scene");

        //Debug.Log("Found " + next.name);
        //Debug.Log("Found " + previous.name);
    }

    // Update is called once per frame
    void Update()
    {
        ////Debug.Log("Manager update");
        //if(refreshColors && mainVectorField.fieldType == fieldTypes[(int)currentScene])
        //{
        //    mainVectorField.preDisplay += RefreshColors;
        //    //Debug.Log("Manager has bound call to refresh");
        //}

        //Debug.Log("Next triggered: " + next.triggered);
        //Debug.Log("Next val: " + next.ReadValue<float>());

        if(next.triggered && (next.ReadValue<float>() != 0))
        {
            currentScene = (Scene) (((int) (currentScene + 1)) % Enum.GetNames(typeof(Scene)).Length); // Not the best way to do this. 
        }
        if(previous.triggered && (previous.ReadValue<float>() != 0))
        {
            currentScene = (Scene)(((int)(currentScene + 8)) % 9);
        }

        if(currentScene != pastScene)
        {
            UpdateScene();
        }
    }

    private void UpdateScene()
    {
        mainVectorField.fieldType = fieldTypes[(int)currentScene];
        //Debug.Log("changed field type.");
        if (currentDetector != null)
        {
            Destroy(currentDetector);
        }
        GameObject detectorPrefab = detectors[(int)currentScene];
        if(detectorPrefab != null)
        {
            currentDetector = Instantiate(detectorPrefab);
            currentDetector.transform.position = spawnPoint.transform.position;
        }

        pastScene = currentScene;

        refreshColors = true;

        StartCoroutine(RefreshColors());
        //mainVectorField.preDisplay += RefreshColors;
    }

    //public void RefreshColors()
    public IEnumerator RefreshColors()
    {
        yield return new WaitForSeconds(0.1f);

        // Raising ArgumentNullException (VectorDisplay.cs line 215)
        //((VectorDisplay)mainVectorField.display).RecalculateMaxMagnitude(); 
        //mainVectorField.preDisplay -= RefreshColors;

        Debug.LogWarning("This function doesn't make sense anymore. Rewrite it.");

        refreshColors = false;
    }
}
