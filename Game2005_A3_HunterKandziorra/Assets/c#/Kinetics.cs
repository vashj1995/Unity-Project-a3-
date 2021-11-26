using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kinetics: MonoBehaviour
{
    public float mass = 1.0f;
    public Vector3 velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Now moving " + gameObject.name + "!");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + (velocity * Time.deltaTime);
    }
}
