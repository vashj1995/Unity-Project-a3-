using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderSphere : ColliderBase
{
    public float radius = 1.0f;
    public override CollisionShape GetCollisionShape()
    {
        return CollisionShape.Sphere;
    }
}
