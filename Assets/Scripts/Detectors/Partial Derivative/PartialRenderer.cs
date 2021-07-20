using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartialRenderer : MonoBehaviour
{
    public ComputeShader renderComputer;
    public ComputeBuffer partialDerivative; // sent from PartialDetector
    public ComputeBuffer positions;

    protected Material bubbleMaterial;
    public Shader bubbleShader;
    [Tooltip("The color of the bubbles produced.")]
    public Color bubbleColor = new Color(0, 1, 0.92f, 1);
    [Tooltip("The mesh to make the bubbles out of.")]
    public Mesh bubbleMesh;

    /// <summary>
    /// A two-entry buffer keeping track of an internal variable used to determine the positions of the particles. 
    /// </summary>
    protected ComputeBuffer distancesBuffer;

    /// <summary>
    /// The distance from the detector center that the streams should start
    /// </summary>
    public float sourceRadius = 0.1f;
    /// <summary>
    /// The distance that the streams should travel before fading. 
    /// </summary>
    public float travelDistance = 0.1f;
    /// <summary>
    /// The number of particles per stream at a given time. 
    /// </summary>
    [SerializeField, Min(1)]
    protected int particlesPerStream = 3;
    /// <summary>
    /// The starting scale of the particles. 
    /// </summary>
    [SerializeField, Min(0)]
    protected float startingScale;

    public Bounds bounds;

    protected bool initialized = false;


    // Start is called before the first frame update
    void Start()
    {
        if (bubbleMaterial == null || bubbleMaterial.shader != bubbleShader)
        {
            bubbleMaterial = new Material(bubbleShader);
        }
        bubbleMaterial.SetColor("_Color", bubbleColor);
        bubbleMaterial.SetInt("_ParticlesPerStream", particlesPerStream);
        bubbleMaterial.SetFloat("_StartDistance", sourceRadius);
        bubbleMaterial.SetFloat("_TravelDistance", travelDistance);
        bubbleMaterial.SetFloat("_StartingSize", startingScale);
    }

    protected void Initialize()
    {
        if(initialized) { return; }

        distancesBuffer = new ComputeBuffer(2, sizeof(float));

        int kernelID = 0;

        renderComputer.SetBuffer(kernelID, "_Partial", partialDerivative);
        renderComputer.SetBuffer(kernelID, "_Distances", distancesBuffer);
        renderComputer.Dispatch(kernelID, 1, 1, 1);

        initialized = true;
    }

    public void CreateDisplay()
    {
        Initialize();

        // Calculate the distances.
        int kernelID = 1;
        renderComputer.SetBuffer(kernelID, "_Partial", partialDerivative);
        renderComputer.SetBuffer(kernelID, "_Distances", distancesBuffer);
        renderComputer.SetFloat("_DeltaTime", Time.deltaTime);
        renderComputer.Dispatch(kernelID, 1, 1, 1);

        //float[] debugArray = new float[2];
        //distancesBuffer.GetData(debugArray);
        //Debug.Log("Distances: " + debugArray[0]);

        // And plot stuff. 
        DisplayBubbles();
    }

    protected void DisplayBubbles()
    {
        bubbleMaterial.SetBuffer("_Distances", distancesBuffer);
        bubbleMaterial.SetBuffer("_Partial", partialDerivative);
        bubbleMaterial.SetVector("_CenterPosition", transform.position);
        bubbleMaterial.SetVector("_Direction", transform.right);

        Graphics.DrawMeshInstancedProcedural(bubbleMesh, 0, bubbleMaterial, bounds, 2 * particlesPerStream);
    }

    private void OnDisable()
    {
        if(distancesBuffer != null)
        {
            distancesBuffer.Release();
            distancesBuffer = null;
        }

        initialized = false;
    }
}
