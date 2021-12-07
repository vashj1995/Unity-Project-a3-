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

        CollisionDetectionUpdate();
    }

    void CollisionDetectionUpdate()
    {
        for (int i = 0; i < CollisionShapes.Count; i++)
        {
            for (int j = 0; j < CollisionShapes.Count; j++)
            {
                CollisionShape shape = CollisionShapes[i].GetCollisionShape();
                if (shape == CollisionShape.Sphere)
                {
                    Color colorA = CollisionShapes[i].GetComponent<Renderer>().material.color;
                    Color colorB = CollisionShapes[j].GetComponent<Renderer>().material.color;

                    CollisionShapes[i].IsCollidingWithSphere((CollisionShapeSphere)CollisionShapes[j]);
                    CollisionShapes[i].GetComponent<Renderer>().material.color = Color.Lerp(colorA, colorB, 0.1f);
                }
            }

        }
    }
}
