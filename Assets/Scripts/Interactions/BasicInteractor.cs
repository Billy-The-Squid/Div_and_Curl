using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicInteractor : MonoBehaviour
{
    public float angularSpeed = 3;

    protected Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rigidBody.angularVelocity = transform.up * angularSpeed;
    }
}
