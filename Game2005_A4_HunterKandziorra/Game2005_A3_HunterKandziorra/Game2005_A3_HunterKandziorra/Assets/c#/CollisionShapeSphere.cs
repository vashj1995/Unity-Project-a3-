using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionShapeSphere : CollisionShapeBase
{
    public float radius = 1.0f;

    public override CollisionResponse CollideWithSphere(CollisionShapeSphere other)
    {
        throw new System.NotImplementedException();
    }

    public override CollisionShape GetCollisionShape()
    {
        return CollisionShape.Sphere;
    }

    public override bool IsCollidingWithSphere(CollisionShapeSphere other)
    {
        Vector3 displacementToOther = (other.transform.position - transform.position);
        float distance = displacementToOther.magnitude;
        float sumRadii = other.radius + radius;
        bool isOverlapping = distance < sumRadii;

        return isOverlapping; 
        //Check Collsion with a Sphere
    }
}

