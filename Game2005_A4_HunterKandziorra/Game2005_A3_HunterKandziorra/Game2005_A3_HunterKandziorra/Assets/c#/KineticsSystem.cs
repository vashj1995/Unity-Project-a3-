using System.Collections.Generic;
using UnityEngine;

public class KineticsSystem : MonoBehaviour
{
    public Vector3 gravity = new Vector3(0, -9.81f, 0);

    public List<Kinetics> Kinetiks;
    public List<CollisionShapeBase> CollisionShapes;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void fixedUpdate()
    {
        for (int i = 0; i < Kinetiks.Count; i++)
        {
            Kinetiks[i].velocity += gravity * Time.fixedDeltaTime;
        }

        CollisionUpdate();
    }

    void CollisionUpdate()
    {
        for (int objectIndexA = 0; objectIndexA < Kinetiks.Count; objectIndexA++)
        {
            for (int objectIndexB = objectIndexA + 1; objectIndexB < Kinetiks.Count; objectIndexB++)
            {
                Kinetics objectA = Kinetiks[objectIndexA];
                Kinetics objectB = Kinetiks[objectIndexB];

                if (objectA.shape == null || objectB.shape == null)
                {
                    continue;
                }
                if(objectB.shape.GetCollisionShape() == CollisionShape.Sphere)
                {
                    bool isOverlapping = objectA.shape.IsCollidingWithSphere((CollisionShapeSphere)objectB.shape);

                    if(isOverlapping)
                    {
                        Debug.Log("Collision");
                        objectA.GetComponent<Renderer>().material.color = new Color (0,1.0f, 1.0f);
                        objectB.GetComponent<Renderer>().material.color = new Color(0, 1.0f, 1.0f);

                    }
                }

                ////if both r spheres
                //if (objectA.shape.GetCollisionShape() == CollisionShape.Sphere && objectB.shape.GetCollisionShape() == CollisionShape.Sphere)
                //{
                //    //does Something ig?
                //    ((CollisionShapeSphere)objectA.shape).IsOverlappingWithSphere((CollisionShapeSphere)objectB.shape);
                //}
            }
        }
    }

    //void CollisionDetectionUpdate()
    //{
    //    for (int i = 0; i < CollisionShapes.Count; i++)
    //    {
    //        for (int j = 0; j < CollisionShapes.Count; j++)
    //        {
    //            CollisionShape shape = CollisionShapes[i].GetCollisionShape();
    //            if (shape == CollisionShape.Sphere)
    //            {
    //                Color colorA = CollisionShapes[i].GetComponent<Renderer>().material.color;
    //                Color colorB = CollisionShapes[j].GetComponent<Renderer>().material.color;

    //                CollisionShapes[i].IsCollidingWithSphere((CollisionShapeSphere)CollisionShapes[j]);
    //                CollisionShapes[i].GetComponent<Renderer>().material.color = Color.Lerp(colorA, colorB, 0.1f);
    //            }
    //        }

    //    }
    //}
}
