using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicsSystem : MonoBehaviour
{
    public float gravityScale = -9.81f;
    public Vector3 gravity = new Vector3(0, 0, 0);
    public List<Kinematics> Kinematiks = new List<Kinematics>();

    //Smallest distance before things get FUNKY
    public float minimumDistance = 0.0001f;

    void Start()
    {
        gravity.y = gravityScale;
    }

    void FixedUpdate()
    {
        for (int i = 0; i < Kinematiks.Count; i++)
        {
            Kinematics obj = Kinematiks[i];

            if (!obj.lockPosition)
            {
                // Velocity Update
                obj.velocity += (gravity * obj.gravityScale) * Time.fixedDeltaTime;

                // Position Update
                obj.transform.position = obj.transform.position + obj.velocity * Time.fixedDeltaTime;
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

    static void ApplyMinimumTraslationVector(Kinematics a, Kinematics b, Vector3 minimumTranslationVectorAtoB, Vector3 collisionNormalAtoB, Vector3 contact)
    {
        //calculate the proper scaler if object is locked or not
        ComputeMovementScalars(a, b, out float moveScalarA, out float moveScalarB);

        // calculate Translations
        Vector3 TranslationVectorA = -minimumTranslationVectorAtoB * moveScalarA;
        Vector3 TranslationVectorB = minimumTranslationVectorAtoB * moveScalarB;

        // Update Positions based on Translations
        a.transform.Translate(TranslationVectorA);
        b.transform.Translate(TranslationVectorB);

        Vector3 contactPoint = contact;

        ApplyVelocityResponse(a, b, collisionNormalAtoB);

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
        //ApplyMinimumTranslationVector(a.kinematicsObject, b.kinematicsObject, minimumTranslationVectorAtoB, collisionNormalAtoB, contactPoint);
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

        //ApplyMinimumTranslationVector(a.kinematicsObject, b.kinematicsObject, mtv, normal, contact);
    }

    static void AABBAABBCollision(ColliderBase objectA, ColliderBase objectB)
    {
        // GetHalf sizes along each axis (x, y, and z)
        // Get distance detween the boxes on each axis (x, y, and z)

        Vector3 halfSizeA = ((AABBControl)objectA).GetHalfSize();
        Vector3 halfSizeB = ((AABBControl)objectB).GetHalfSize();

        Vector3 displacementAtoB = objectB.transform.position - objectA.transform.position;

        float distX = Mathf.Abs(displacementAtoB.x);
        float distY = Mathf.Abs(displacementAtoB.y);
        float distZ = Mathf.Abs(displacementAtoB.z);

        // For each axis:
        // If the distance between the boxes (along the axis) is less than the sum of the half sizes
        // then they are overlapping

        float penetrationX = halfSizeA.x + halfSizeB.x - distX;
        float penetrationY = halfSizeA.y + halfSizeB.y - distY;
        float penetrationZ = halfSizeA.z + halfSizeB.z - distZ;

        // If there is an overlap along ALL axis then they are colliding, else they are not

        if (penetrationX < 0 || penetrationY < 0 || penetrationZ < 0)
        {
            return;
        }

        // Find minimumTraslationVector (i.e. what is the shortest path we can take)
        // Along which axis are they closest to being seperate
        // Move along that axis according to how much overlap there is

        Vector3 minimumTranslationVector;
        Vector3 collisionNormalAtoB;
        Vector3 contact;

        if (penetrationX < penetrationY && penetrationX < penetrationZ) // is penX the shortest?
        {
            collisionNormalAtoB = new Vector3(Mathf.Sign(displacementAtoB.x), 0, 0);    // Sign returns -1 or 1 based on sign
            minimumTranslationVector = collisionNormalAtoB * penetrationX;
        }
        else if (penetrationY < penetrationX && penetrationY < penetrationZ) // is penY the shortest?
        {
            collisionNormalAtoB = new Vector3(0, Mathf.Sign(displacementAtoB.y), 0);    // Sign returns -1 or 1 based on sign
            minimumTranslationVector = collisionNormalAtoB * penetrationY;
        }
        else //if (penetrationZ < penetrationY && penetrationZ < penetrationX) // is penZ the shortest?   // could just be else
        {
            collisionNormalAtoB = new Vector3(0, 0, Mathf.Sign(displacementAtoB.z));    // Sign returns -1 or 1 based on sign
            minimumTranslationVector = collisionNormalAtoB * penetrationZ;
        }

        contact = objectA.transform.position + minimumTranslationVector;

        ApplyMinimumTraslationVector(objectA.kinematicsObject, objectB.kinematicsObject, minimumTranslationVector, collisionNormalAtoB, contact);

    }


    static void SphereAABBCollision(ColliderBase sphere, ColliderBase box)
    {
        // GetHalf sizes along each axis (x, y, and z)
        // Get distance between the sphere and box on each axis (x, y, and z)

        float Raduis = (((ColliderSphere)sphere).radius);
        Vector3 halfSizeB = ((AABBControl)box).GetHalfSize();

        Vector3 displacementAtoB = box.transform.position - sphere.transform.position;

        float distX = Mathf.Abs(displacementAtoB.x);
        float distY = Mathf.Abs(displacementAtoB.y);
        float distZ = Mathf.Abs(displacementAtoB.z);

        // For each axis:
        // If the distance between the boxes (along the axis) is less than the sum of the half sizes
        // then they are overlapping

        float penetrationX = Raduis + halfSizeB.x - distX;
        float penetrationY = Raduis + halfSizeB.y - distY;
        float penetrationZ = Raduis + halfSizeB.z - distZ;

        // If there is an overlap along ALL axis then they are colliding, else they are not

        if (penetrationX < 0 || penetrationY < 0 || penetrationZ < 0)
        {
            return;
        }

        // Find minimumTraslationVector (i.e. what is the shortest path we can take)
        // Along which axis are they closest to being seperate
        // Move along that axis according to how much overlap there is

        Vector3 minimumTranslationVector;
        Vector3 collisionNormalAtoB;
        Vector3 contact;

        if (penetrationX < penetrationY && penetrationX < penetrationZ) // is penX the shortest?
        {
            collisionNormalAtoB = new Vector3(Mathf.Sign(displacementAtoB.x), 0, 0);    // Sign returns -1 or 1 based on sign
            minimumTranslationVector = collisionNormalAtoB * penetrationX;
        }
        else if (penetrationY < penetrationX && penetrationY < penetrationZ) // is penY the shortest?
        {
            collisionNormalAtoB = new Vector3(0, Mathf.Sign(displacementAtoB.y), 0);    // Sign returns -1 or 1 based on sign
            minimumTranslationVector = collisionNormalAtoB * penetrationY;
        }
        else //if (penetrationZ < penetrationY && penetrationZ < penetrationX) // is penZ the shortest?   // could just be else
        {
            collisionNormalAtoB = new Vector3(0, 0, Mathf.Sign(displacementAtoB.z));    // Sign returns -1 or 1 based on sign
            minimumTranslationVector = collisionNormalAtoB * penetrationZ;
        }

        contact = sphere.transform.position + minimumTranslationVector;

        ApplyMinimumTraslationVector(sphere.kinematicsObject, box.kinematicsObject, minimumTranslationVector, collisionNormalAtoB, contact);
    }


    static void ComputeMovementScalars(Kinematics a, Kinematics b, out float mtvScalarA, out float mtvScalarB)
    {
        // Check to see if either object is Locked
        if (a.lockPosition && !b.lockPosition)
        {
            //if A is locked and B is not
            mtvScalarA = 0.0f;
            mtvScalarB = 1.0f;
            return;
        }
        if (!a.lockPosition && b.lockPosition)
        {
            //if B is locked and A is not
            mtvScalarA = 1.0f;
            mtvScalarB = 0.0f;
            return;
        }
        if (!a.lockPosition && !b.lockPosition)
        {
            //if both objects are not locked
            mtvScalarA = 0.5f;
            mtvScalarB = 0.5f;
            return;
        }
        //else is A and B is locked
        mtvScalarA = 0.0f;
        mtvScalarB = 0.0f;
    }

    static void ApplyVelocityResponse(Kinematics objA, Kinematics objB, Vector3 collisionNormal)
    {
        Vector3 normal = collisionNormal;

        // Velocity of B relative to A
        Vector3 relativeVelocityAB = objB.velocity - objA.velocity;

        // Find relative velocity
        float relativeNormalVelocityAB = Vector3.Dot(relativeVelocityAB, normal);

        // Early exit if they are not going towards each other (no bounce)
        if (relativeNormalVelocityAB >= 0.0f)
        {
            return;
        }

        // Choose a coefficient of restitution
        float restitution = (objA.bounce + objB.bounce) * 0.5f;

        float deltaV;

        float minimumRelativeVelocityForBounce = 3.0f;

        // If we only need the objects to slide and not bounce, then...
        if (relativeNormalVelocityAB < -minimumRelativeVelocityForBounce)
        {
            // Determine change in velocity 
            deltaV = (relativeNormalVelocityAB * (1.0f + restitution));
        }
        else
        {
            // no bounce
            deltaV = (relativeNormalVelocityAB);
        }

        float impulse;
        // respond differently based on locked states
        if (objA.lockPosition && !objB.lockPosition)
        {
            // Only B
            impulse = -deltaV * objB.mass;
            objB.velocity += normal * (impulse / (objB.mass));
        }
        else if (!objA.lockPosition && objB.lockPosition)
        {
            // impulse required to creat our desired change in velocity
            // impulse = Force * time = kg * m/s^2 * s = kg m/s
            // impulse / objA.mass == deltaV
            // Only A change velocity
            impulse = -deltaV * objA.mass;
            objA.velocity -= normal * (impulse / (objA.mass));
        }
        else if (!objA.lockPosition && !objB.lockPosition)
        {
            // Both
            impulse = deltaV / ((1.0f / objA.mass) + (1.0f / objB.mass));
            objA.velocity += normal * (impulse / objA.mass);
            objB.velocity -= normal * (impulse / objB.mass);
        }
        else if (!objA.lockPosition && !objB.lockPosition)
        {
            // Nadda
        }
        else
        {
            return;
        }

        // subtract the component of relative velocity that is along the normal of the collision to receive the tangential velocity
        Vector3 relativeSurfaceVelocity = relativeVelocityAB - (relativeNormalVelocityAB * normal);

        ApplyFriction(objA, objB, relativeSurfaceVelocity, normal);
    }

    static void ApplyFriction(Kinematics a, Kinematics b, Vector3 relativeSurfaceVelocityAtoB, Vector3 normalAtoB)
    {
        // Need both objects
        // Need relative surface velocity between objects to know which direction to apply the friction force


        float minFrictionSpeed = 0.0001f;
        float relativeSpeed = relativeSurfaceVelocityAtoB.magnitude;

        // Only apply friction if the relative velocity is significant
        if (relativeSurfaceVelocityAtoB.sqrMagnitude < minFrictionSpeed)
        {
            return;
        }

        float kFrictionCoefficient = (a.friction + b.friction) * 0.5f;

        Vector3 directionToApplyFriction = relativeSurfaceVelocityAtoB / relativeSpeed; // normalizing

        Vector3 gravity1 = new Vector3(0.0f, -9.81f, 0.0f); // Not Sure Why I can't Access Gravity??________________________________???

        float gravityAccelerationAlongNormal = Vector3.Dot(gravity1, normalAtoB);    // * by mass to find force

        Vector3 frictionAcceleration = directionToApplyFriction * gravityAccelerationAlongNormal * kFrictionCoefficient;
        if (!a.lockPosition)
        {
            a.velocity += frictionAcceleration * Time.fixedDeltaTime;   // didn't divide by mass, but could have if we multiplied by mas earlier
        }
        if (!b.lockPosition)
        {
            b.velocity += frictionAcceleration * Time.fixedDeltaTime;
        }
    }
}