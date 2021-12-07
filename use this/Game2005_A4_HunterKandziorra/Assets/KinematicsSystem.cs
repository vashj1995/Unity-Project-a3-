using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicsSystem : MonoBehaviour
{
    public Vector3 gravity = new Vector3(0, -9.81f, 0);
    public List<Kinematics> Kinematiks = new List<Kinematics>();

    //Smallest distance before things get FUNKY
    public float minimumDistance = 0.0001f;

    void FixedUpdate()
    {
        //Velocity update
        for (int i = 0; i < Kinematiks.Count; i++)
        {
            Kinematics obj = Kinematiks[i];
            if (!obj.lockPosition)
            {
                obj.velocity += gravity * obj.gravityScale * Time.fixedDeltaTime;
            }
        }

        //Position update
        for (int i = 0; i < Kinematiks.Count; i++)
        {
            Kinematics obj = Kinematiks[i];

            if (!obj.lockPosition)
            {
                obj.transform.position += obj.velocity * Time.fixedDeltaTime;
            }
        }


        CollisionUpdate();
    }

    void CollisionUpdate()
    {
        for (int objectIndexA = 0; objectIndexA < Kinematiks.Count; objectIndexA++)
        {
            for (int objectIndexB = objectIndexA + 1; objectIndexB < Kinematiks.Count; objectIndexB++)
            {
                Kinematics objectA = Kinematiks[objectIndexA];
                Kinematics objectB = Kinematiks[objectIndexB];

                //If one does not have a collider...
                if (objectA.shape == null || objectB.shape == null)
                {
                    continue;
                }

                //If both are spheres...
                //GetCollisionShape is defined in the base class to allow us to determine what derived classes to cast to
                if (objectA.shape.GetCollisionShape() == CollisionShape.Sphere &&
                    objectB.shape.GetCollisionShape() == CollisionShape.Sphere)
                {
                    //FizziksObject.shape is a base class reference to FizziksColliderBase,
                    //but to do specific things with it, we need to cast to our derived class FizziksColliderSphere
                    SphereSphereCollision((ColliderSphere)objectA.shape, (ColliderSphere)objectB.shape);
                }

                if (objectA.shape.GetCollisionShape() == CollisionShape.Sphere &&
                    objectB.shape.GetCollisionShape() == CollisionShape.Plane)
                {
                    SpherePlaneCollision((ColliderSphere)objectA.shape, (ColliderPlane)objectB.shape);
                }

                if (objectA.shape.GetCollisionShape() == CollisionShape.Plane &&
                    objectB.shape.GetCollisionShape() == CollisionShape.Sphere)
                {
                    SpherePlaneCollision((ColliderSphere)objectB.shape, (ColliderPlane)objectA.shape);
                }
            }
        }
    }

    //In C++ we can return more than one thing at a time using reference parameters with &
    //In C#, we can define "out" parameters, which allows us to return more than one thing
    void GetLockedMovementScalars(Kinematics a, Kinematics b, out float movementScalarA, out float movementScalarB)
    {
        //If A is locked and B is not
        // A*0
        // B*1
        if (a.lockPosition && !b.lockPosition)
        {
            movementScalarA = 0.0f;
            movementScalarB = 1.0f;
            return;
        }

        //If B is locked and A is not
        // A*1
        // B*0
        if (!a.lockPosition && b.lockPosition)
        {
            movementScalarA = 1.0f;
            movementScalarB = 0.0f;
            return;
        }

        //If neither are locked
        // A*0.5
        // B*0.5
        if (!a.lockPosition && !b.lockPosition)
        {
            movementScalarA = 0.5f;
            movementScalarB = 0.5f;
            return;
        }

        //If both are locked
        // A*0.0
        // B*0.0
        movementScalarA = 0.0f;
        movementScalarB = 0.0f;
    }

    void SphereSphereCollision(ColliderSphere a, ColliderSphere b)
    { 
        // do sphere-sphere collision detection
        // note: sphere-sphere collision detection is the same as circle-circle.
        // If the distance between spheres is less than the sum of their radii, then they are overlapping
        Vector3 displacment = b.transform.position - a.transform.position;
        float distance = displacment.magnitude;
        float sumRadii = a.radius + b.radius;
        float penetrationDepth = sumRadii - distance;
        bool isOverlapping = penetrationDepth > 0;

        if (!isOverlapping)
        {
            return;
        }

        //Mix colors (just to make it easier to see when objects are touching)
        {
            Debug.Log(a.name + " collided with: " + b.name);
            Color colorA = a.GetComponent<Renderer>().material.color;
            Color colorB = b.GetComponent<Renderer>().material.color;
            a.GetComponent<Renderer>().material.color = Color.Lerp(colorA, colorB, 0.05f);
            b.GetComponent<Renderer>().material.color = Color.Lerp(colorB, colorA, 0.05f);
        }

        Vector3 collisionNormalAtoB;
        if (distance < minimumDistance)
        {
           // distance = minimumDistance;
            collisionNormalAtoB = new Vector3(0, penetrationDepth, 0);
        }
        else
        {
            collisionNormalAtoB = displacment / distance;
        }

        //Our minimum translation vector is the vector we have to move along so the objects no longer overlap
        Vector3 minimumTranslationVectorAtoB = penetrationDepth * collisionNormalAtoB;
        Vector3 contactPoint = a.transform.position + collisionNormalAtoB * a.radius;
        ApplyMinimumTranslationVector(a.kinematicsObject, b.kinematicsObject, minimumTranslationVectorAtoB,
            collisionNormalAtoB, contactPoint);
    }

    void SpherePlaneCollision(ColliderSphere a, ColliderPlane b)
    {

        Vector3 somePointOnThePlane = b.transform.position;
        Vector3 centerOfSphere = a.transform.position;

        //Construct any vector from the plane to the center of the sphere
        Vector3 fromPlaneToSphere = centerOfSphere - somePointOnThePlane;

        //Use dot product to find the length of the projection of the center of the sphere sphere onto the plane normal
        //This gives the shortest distance from the plane to the center of the sphere.
        //The sign of this dot product indicates which side of the normal this fromPlaneToSphere vector is on.
        //If the sign is negative, they point in opposite directions
        //If the sign is positive, they are at least somewhat in the same direction
        float dot = Vector3.Dot(fromPlaneToSphere, b.GetNormal());
       
        //float distance = Mathf.Abs(dot);
        float distance = dot; // Abs(dot) will do plane collision.
                              // Removing the Absolute value calculation technically makes it a collision of a "half-space"

        //If the distance is less than the radius of the sphere, they are overlapping
        float penetrationDepth = a.radius - distance;
        bool isOverlapping = penetrationDepth > 0;

        if (!isOverlapping)
        {
            return;
        }

        //Mix colors (just to make it easier to see when objects are touching)
        {
            Debug.Log(a.name + " collided with: " + b.name);
            Color colorA = a.GetComponent<Renderer>().material.color;
            Color colorB = b.GetComponent<Renderer>().material.color;
            a.GetComponent<Renderer>().material.color = Color.Lerp(colorA, colorB, 0.05f);
            b.GetComponent<Renderer>().material.color = Color.Lerp(colorB, colorA, 0.05f);
        }

        Vector3 normal = -b.GetNormal();
        Vector3 mtv = normal * penetrationDepth;
        Vector3 contact = centerOfSphere + (dot * normal);

        Debug.DrawLine(centerOfSphere, contact);
        Debug.DrawRay(contact, normal, Color.red);

        ApplyMinimumTranslationVector(
            a.kinematicsObject,
            b.kinematicsObject,
            mtv,
            normal,
            contact);
    }
    private void ApplyMinimumTranslationVector(Kinematics a, Kinematics b, Vector3 minimumTranslationVectorAtoB, Vector3 normal, Vector3 contactPoint)
    {
        GetLockedMovementScalars(a, b, out float movementScalarA, out float movementScalarB);

        Vector3 translationVectorA = -minimumTranslationVectorAtoB * movementScalarA;
        Vector3 translationVectorB = minimumTranslationVectorAtoB * movementScalarB;

        a.transform.position += translationVectorA;
        b.transform.position += translationVectorB;

        CollisionInfo collisionInfo;
        collisionInfo.objectA = a.shape;
        collisionInfo.objectB = b.shape;
        collisionInfo.collisionNormalAtoB = normal;
        collisionInfo.contactPoint = contactPoint;
    }
}