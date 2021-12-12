using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AABBControl : ColliderBase
{
    public Vector3 GetMin()
    {
        return transform.position - GetHalfSize();
    }

    public Vector3 GetMax()
    {
        return transform.position + GetHalfSize();
    }

    public Vector3 GetSize()
    {
        return transform.lossyScale;
    }
    public Vector3 GetHalfSize()
    {
        return transform.lossyScale * 0.5f;
    }

    public override CollisionShape GetCollisionShape()
    {
        return CollisionShape.AABB;
    }
}