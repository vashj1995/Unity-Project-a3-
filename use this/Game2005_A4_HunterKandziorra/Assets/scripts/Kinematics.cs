using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kinematics : MonoBehaviour
{
    public float mass = 1.0f;
    public Vector3 velocity = Vector3.zero;
    public float gravityScale = 1.0f;
    [Range(0, 1)]
    public string ballName;
    public KinematicsSystem KinematiksSystem;
    public float bounce = 0.6f;
    [Range(0, 1)]
    public float friction = 0.5f;
    private float time = 0.0f;

    //If true, this object will not be moved by the KinematicsSystem
    public bool lockPosition = false;

    // In C# All class members are reference types.
    // This is a base class reference which will be assigned derived class instances
    public ColliderBase shape = null; 

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hello World from " + gameObject.name + "!");
        KinematiksSystem = FindObjectOfType<KinematicsSystem>(); // return the first found component in the scene which has the type
        KinematiksSystem.Kinematiks.Add(this);
    }
    void Update()
    {
        time += Time.deltaTime;
        float small = 0.1f;
        //check every 3 seconds if the objects velocity is smaller than 0.1f
        //to stop the object from infinitely moving in small amounts
        if (time >= 3.0f && (velocity.x <= small || velocity.x >= -small))
        {
            velocity.x = 0.0f;
            time = 0.0f;
        }
        if (time >= 3.0f && (velocity.y <= small || velocity.y >= -small))
        {
            velocity.y = 0.0f;
            time = 0.0f;
        }
        if (time >= 3.0f && (velocity.z <= small || velocity.z >= -small))
        {
            velocity.z = 0.0f;
            time = 0.0f;
        }

    }
}
