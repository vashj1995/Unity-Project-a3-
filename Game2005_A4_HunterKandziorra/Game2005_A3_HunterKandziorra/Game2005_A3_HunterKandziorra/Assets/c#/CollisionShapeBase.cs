using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollisionShape
{
    Sphere = 0,
    Plane = 1, 
    AABB = 2
}

public struct CollisionResponse
{
    public bool didCollide;
    public Vector3 collisionNormal;
    public Vector3 displacementBetweenObjects;
}

public abstract class CollisionShapeBase : MonoBehaviour
{
    private void Start()
    {
        KineticsSystem kineticsSystem = FindObjectOfType<KineticsSystem>();
        kineticsSystem.CollisionShapes.Add(this);
    }

    public abstract CollisionShape GetCollisionShape();

    public abstract bool IsCollidingWithSphere(CollisionShapeSphere other);

    public abstract CollisionResponse CollideWithSphere(CollisionShapeSphere other);
}
