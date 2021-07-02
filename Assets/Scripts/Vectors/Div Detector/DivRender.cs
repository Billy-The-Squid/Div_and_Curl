using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivRender : MonoBehaviour
{
    ///// <summary>
    ///// The shader used to instantiate the particles
    ///// </summary>
    //[SerializeField]
    //Shader shader;

    /// <summary>
    /// The material used to instantiate the particles
    /// </summary>
    [SerializeField]
    protected Material material;

    /// <summary>
    /// The mesh to make the particles out of.
    /// </summary>
    [SerializeField]
    Mesh mesh;

    /// <summary>
    /// The compute buffer in which the values of the divergence are stored.
    /// 
    /// This should be formatted the same as <cref>DivergenceDetector</cref>'s divBuffer.
    /// </summary>
    public ComputeBuffer divBuffer;

    /// <summary>
    /// The buffer in which the position of each particle is stored. 
    /// </summary>
    protected ComputeBuffer posBuffer;
    /// <summary>
    /// The buffer in which the size of each particle is stored.
    /// </summary>
    protected ComputeBuffer sizeBuffer;
    /// <summary>
    /// A six-entry buffer keeping track of an internal variable used to determine the positions of the particles. 
    /// </summary>
    // NOT NECESSARY?

    /// <summary>
    /// The compute shader that calculates the positions each frame. 
    /// </summary>
    [SerializeField]
    protected ComputeShader positionComputer;
    /// <summary>
    /// The distance from the detector center that the streams should start
    /// </summary>
    public float radius;
    /// <summary>
    /// The distance that the streams should travel before fading. 
    /// </summary>
    public float travelDistance;

    /// <summary>
    /// The number of particles per stream at a given time. 
    /// </summary>
    [SerializeField, Min(1)]
    protected int particlesPerStream;

    /// <summary>
    /// The starting scale of the particles. 
    /// </summary>
    [SerializeField, Min(0)]
    protected float startingScale;




    // Start is called before the first frame update
    void Start()
    {
        //// Initialize the material
        //material = new Material(shader);
    }

    private void OnEnable()
    {
        // Initialize the position buffer.
        unsafe {
            posBuffer = new ComputeBuffer(particlesPerStream * 6, sizeof(Vector3));
        }
        sizeBuffer = new ComputeBuffer(particlesPerStream * 6, sizeof(float));

        Initialize();
    }

    private void OnDisable()
    {
        if(posBuffer != null) {
            posBuffer.Release();
            posBuffer = null;
        }
        if(sizeBuffer != null) {
            sizeBuffer.Release();
            sizeBuffer = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(this.enabled == false) { return; }

        int kernelID = 1;

        // Update the positions and sizes
        positionComputer.SetBuffer(kernelID, "_Positions", posBuffer); // RW
        positionComputer.SetBuffer(kernelID, "_Sizes", sizeBuffer); // RW
        positionComputer.SetBuffer(kernelID, "_Divergence", divBuffer);

        positionComputer.SetInt("_ParticlesPerStream", particlesPerStream);
        positionComputer.SetFloat("_StartDistance", radius);
        positionComputer.SetFloat("_TravelDistance", travelDistance);
        positionComputer.SetFloat("_StartingSize", startingScale * transform.localScale.x);

        positionComputer.SetVector("_CenterPosition", transform.position);
        positionComputer.SetFloat("_DeltaTime", Time.deltaTime);

        int numGroups = Mathf.CeilToInt(particlesPerStream / 4f);
        positionComputer.Dispatch(kernelID, 1, numGroups, 1);

        // Display the particles
        material.SetBuffer("_Positions", posBuffer);
        material.SetBuffer("_Sizes", sizeBuffer);

        Bounds bounds = new Bounds(transform.position, radius * travelDistance * Vector3.one * 2);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, 6 * particlesPerStream);
    }



    /// <summary>
    /// Sets up the system and the relevant variables. Should only be called once.
    /// </summary>
    private void Initialize()
    {
        // I'm like 70% sure that divBuffer should already be set, unless we start inside a field. Then who knows. 

        int kernelID = 0;
        // Set the initial positions of each particle. 
        positionComputer.SetBuffer(kernelID, "_Positions", posBuffer); // RW
        positionComputer.SetBuffer(kernelID, "_Sizes", sizeBuffer); // RW
        positionComputer.SetBuffer(kernelID, "_Divergence", divBuffer);

        positionComputer.SetInt("_ParticlesPerStream", particlesPerStream);
        positionComputer.SetFloat("_StartDistance", radius);
        positionComputer.SetFloat("_TravelDistance", travelDistance);
        positionComputer.SetFloat("_StartingSize", startingScale * transform.localScale.x);

        positionComputer.SetVector("_CenterPosition", transform.position);

        positionComputer.Dispatch(kernelID, 1, 1, 1);

        //// Debug code
        //Vector3[] debugArray = new Vector3[posBuffer.count];
        //float[] debugFloatArray = new float[sizeBuffer.count];
        //posBuffer.GetData(debugArray);
        //sizeBuffer.GetData(debugFloatArray);
    }
}
