using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicInteractor : Grabbable
{
    public float angularSpeed = 2;

    public Rigidbody rigidBody;

    private static string nameToDisplay = "Thing";
    private static string description = "A thing. What does it do?";
    private static int index;

    public override string displayName { get { return nameToDisplay; } set => throw new System.NotImplementedException("I'm not allowing name changing right now."); }
    public override string displayDescription { get { return description; } set => throw new System.NotImplementedException("I'm not allowing description changing right now"); }
    public override int menuIndex { get { return index; } set { index = value; } }




    // Start is called before the first frame update
    void Start()
    {
        //rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rigidBody.angularVelocity = transform.up * angularSpeed;
    }
}
