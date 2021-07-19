using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartialRenderer : MonoBehaviour
{
    public ComputeShader renderComputer;
    public ComputeBuffer partialDerivative;
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

    protected bool initialized = false;


    // Start is called before the first frame update
    void Start()
    {
        if (bubbleMaterial == null || bubbleMaterial.shader != bubbleShader)
        {
            bubbleMaterial = new Material(bubbleShader);
        }
        bubbleMaterial.SetColor("_Color", bubbleColor);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        distancesBuffer = new ComputeBuffer(2, sizeof(float));
    }

    private void OnDisable()
    {
        if(distancesBuffer != null)
        {
            distancesBuffer.Release();
            distancesBuffer = null;
        }
    }
}
