using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum KineticsShape
//{
//    Sphere = 0,
//    Plane = 1,
//    AABB = 2
//}

public class Kinetics: MonoBehaviour
{
    public float mass = 1.0f;
    public Vector3 velocity = Vector3.zero;
    //public float size = 1.0f;
    public CollisionShapeBase shape = null;

    //public EnumCollisionShape shape;

    // Start is called before the first frame update
    void Start()
    {
        KineticsSystem kineticsSystem = FindObjectOfType<KineticsSystem>();
        kineticsSystem.Kinetiks.Add(this);

        //GetComponent<CollisionShapeBase>()

        Debug.Log("Now moving " + gameObject.name + "!");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + (velocity * Time.deltaTime);
    }
}
