using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
public class Fishes : MonoBehaviour
{
    /// <summary>
    /// The character controller. 
    /// </summary>
    protected CharacterController controller;

    /// <summary>
    /// The maximum distance in the x or z direction the fish can wander from the center of the map.
    /// </summary>
    public float maxDistanceFromCenter = 20f;

    /// <summary>
    /// The maximum time the fish will wait before moving again.
    /// </summary>
    public float maxWaitTime = 1.5f;

    /// <summary>
    /// The maximum speed at which the fish will drift.
    /// </summary>
    public float maxDriftSpeed = 0.1f;

    /// <summary>
    /// The maximum speed at which the fish can dart.
    /// </summary>
    public float maxDartSpeed = 1f;

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

    /// <summary>
    /// Current speed, used for accelerating. 
    /// </summary>
    protected float currentSpeed;

    /// <summary>
    /// The acceleration at the beginning and end of the dart.
    /// </summary>
    [SerializeField]
    protected float dartAcceleration = 1;

    /// <summary>
    /// The time at which the fish stops turning. 
    /// </summary>
    protected float endTurnTime;

    /// <summary>
    /// The speed at which the fish turns
    /// </summary>
    public float turnSpeed = 180;

    /// <summary>
    /// +1 to turn right, -1 to turn left. 
    /// </summary>
    protected float turnDirection;

    /// <summary>
    /// The speed at which the fish moves while it's turning. 
    /// </summary>
    public float turnMoveSpeed = 0.5f;





    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();

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
        if(state == State.NotMoving) // We should really just make a choice between waiting/drift, darting, and turning. 
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
                endDartTime = endWaitTime + 1; //Random.Range(0f, maxDartDuration); // Do this better. 
            } else
            {
                turnDirection = Mathf.Floor(Random.Range(0.5f, 1.5f)) * 2 - 1;
                endTurnTime = endWaitTime + Random.Range(0f, 1); // Do this better.
            }

            state = State.Waiting;

            // Debug code:
            Debug.Log("Not moving. Next movement type: " + (movementModeDart ? "dart" : "turn"));
            if(movementModeDart)
            {
                Debug.Log("Dart speed: " + dartSpeed);
            }
        } 
        else if (state == State.Waiting)
        {
            controller.Move(driftVelocity * Time.deltaTime);

            if(Time.time > endWaitTime)
            {
                state = State.Moving;

                // Debug code:
                Debug.Log("Done waiting");
            }
        }
        else // State.Moving
        {
            // Dart motion
            if(movementModeDart) {
                // Accelerate
                if(Time.time > endDartTime) {
                    currentSpeed -= dartAcceleration * Time.deltaTime;
                } 
                else if(currentSpeed < dartSpeed) {
                    currentSpeed += dartAcceleration * Time.deltaTime;
                }

                // Is this the end of the dart?
                if(currentSpeed <= 0)
                {
                    state = State.NotMoving;
                    currentSpeed = 0;
                    return;
                }

                // Move
                controller.Move(transform.forward * currentSpeed * Time.deltaTime);

                // Debug code
                Debug.Log("Current speed: " + currentSpeed);
            }

            // Turn motion
            else
            {
                if (Time.time < endTurnTime)
                {
                    transform.Rotate(new Vector3(0, 1, 0), turnSpeed * Time.deltaTime * turnDirection);
                    controller.Move(transform.forward * Time.deltaTime * turnMoveSpeed);
                }
                else
                {
                    state = State.NotMoving;
                }
            }
        }
    }
}
