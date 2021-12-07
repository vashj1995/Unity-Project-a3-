using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kinematics : MonoBehaviour
{
    public float mass = 1.0f;
    public Vector3 velocity = Vector3.zero;
    public float gravityScale = 1.0f;

    //If true, this object will not be moved by our Fizziks system
    public bool lockPosition = false;

    // In C# All class members are reference types.
    // This is a base class reference which will be assigned derived class instances
    public ColliderBase shape = null; 

    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<KinematicsSystem>().Kinematiks.Add(this);
        shape = GetComponent<ColliderBase>();
        // Debug.Log("Hello World from " + gameObject.name + "!");
    }
}
