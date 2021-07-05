using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Fishes : MonoBehaviour
{
    /// <summary>
    /// The maximum distance in the x or z direction the fish can wander from the center of the map.
    /// </summary>
    public float maxDistanceFromCenter = 20f;

    /// <summary>
    /// The maximum time the fish will wait before moving again.
    /// </summary>
    public static float maxWaitTime = 1.5f;

    /// <summary>
    /// The maximum speed at which the fish will drift.
    /// </summary>
    public static float maxDriftSpeed = 0.1f;

    /// <summary>
    /// The maximum speed at which the fish can dart.
    /// </summary>
    public static float maxDartSpeed = 1f;

    protected enum State { NotMoving, Waiting, Moving }
    /// <summary>
    /// Is the fish currently moving or waiting to move?
    /// 
    /// NotMoving indicates that the fish has finished moving and needs its next variables set. 
    /// Waiting indicates that the fish is pausing before resuming swimming. 
    /// Moving indicates that the fish is executing its movement. 
    /// </summary>
    protected State state;

    /// <summary>
    /// Time at which wait stops. 
    /// </summary>
    protected float endWaitTime;

    /// <summary>
    /// The velocity of the fish's drift.
    /// </summary>
    protected Vector3 driftVelocity;

    /// <summary>
    /// If false, the fish will turn. If true, the fish will dart. 
    /// </summary>
    protected bool movementModeDart;

    /// <summary>
    /// The speed at which the fish will dart. 
    /// </summary>
    protected float dartSpeed;

    /// <summary>
    /// Time at which the fish stops darting. 
    /// </summary>
    protected float endDartTime;





    // Start is called before the first frame update
    void Start()
    {
        // Set the initial position
        Vector3 pos = new Vector3(0, 0, 0);
        pos.x = Random.Range(-maxDistanceFromCenter, maxDistanceFromCenter);
        pos.y = transform.position.y;
        pos.z = Random.Range(-maxDistanceFromCenter, maxDistanceFromCenter);
        transform.position = pos;

        // Set the initial direction
        Vector2 facing = Random.insideUnitCircle;
        Vector3 rot = new Vector3(facing.x, 0, facing.y);
        transform.forward = rot;
    }

    // Update is called once per frame
    void Update()
    {
        if(state == State.NotMoving)
        {
            // Select wait time
            endWaitTime = Time.time + Random.Range(0f, maxWaitTime);
            // Select drift direction (just forwards or backwards)
            driftVelocity = transform.forward * Random.Range(-maxDriftSpeed, maxDriftSpeed);
            // Alternate the movement mode (turn vs dart)
            movementModeDart = Mathf.FloorToInt(Random.Range(0.5f, 1.5f)) == 0;
            // Select movement parameters (distance, speed)
            if(movementModeDart)
            {
                dartSpeed = Random.Range(0f, maxDartSpeed);
                float normV = dartSpeed / maxDartSpeed;
                float maxDartDuration = -normV * (normV - 1); // Improve this function.
                endDartTime = endWaitTime + Random.Range(0f, maxDartDuration);
            } else
            {
                //turnSpeed = Random.Range(0f, maxTurnSpeed);
                //float normW = turnSpeed / maxTurnSpeed;
                //float turnDura
            }

            state = State.Waiting;
        } 
        else if (state == State.Waiting)
        {


            if(Time.time > endWaitTime)
            {
                state = State.Moving;
            }
        }
        else // State.Moving
        {

        }
    }
}
