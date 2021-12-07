using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis
{
    X = 0,
    Y,
    Z
}

public class CollisionShapePlane : CollisionShapeBase
{
    public float distanceDebug;
    public Axis alignment = Axis.Y; 
    public Vector3 getNormal()
    {
        switch(alignment)
        {
            case (Axis.X):
                {
                    return transform.right;
                }
            case (Axis.Y):
                {
                    return transform.up;
                }
            case (Axis.Z):
                {
                    return transform.forward;
                }
            default:
                {
                    throw new System.Exception("Invalid alignment?");
                }
        }
    }

    public override CollisionShape GetCollisionShape()
    {
        return CollisionShape.Plane;
    }

    public override bool IsCollidingWithSphere(CollisionShapeSphere other)
    {
        //Find the distance between the center of the sphere and the plane
        Vector3 planeNormal = getNormal();
        Vector3 displacementFromPlaneToSphere = other.transform.position - transform.position;
        float dotProduct = Vector3.Dot(planeNormal, displacementFromPlaneToSphere);
        float distanceFromPlaneToSphere = Mathf.Abs(dotProduct);
        distanceDebug = dotProduct;


        bool isOverlapping = distanceFromPlaneToSphere < other.radius ;
        return isOverlapping;
    }

    public override CollisionResponse CollideWithSphere(CollisionShapeSphere other)
    {
        throw new System.NotImplementedException();
    }
}
