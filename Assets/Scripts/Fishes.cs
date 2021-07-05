using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
public class Fishes : MonoBehaviour
{
    /// <summary>
    /// The character controller that moves the fish. 
    /// </summary>
    protected CharacterController controller;

    /// <summary>
    /// The movement speed of this fish
    /// </summary>
    public float moveSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        float direction = Mathf.Floor(Random.Range(0.5f, 1.5f)) * 2 - 1;
        transform.forward = (Vector3.Cross(transform.position, Vector3.up) * direction).normalized;

        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        controller.Move(transform.forward * Time.deltaTime * moveSpeed);
        transform.forward = (Vector3.Cross(transform.position, Vector3.up)).normalized;
    }
}
