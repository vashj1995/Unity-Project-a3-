using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KineticsSystem : MonoBehaviour
{
    public Vector3 gravity = new Vector3(0, -9.81f, 0);

    public List<Kinetics> Kinetiks;

    // Start is called before the first frame update
    void Start()
    {
        for (int i=0; i< Kinetiks.Count; i++)
        {
            Kinetiks[i].velocity += gravity * Time.deltaTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
