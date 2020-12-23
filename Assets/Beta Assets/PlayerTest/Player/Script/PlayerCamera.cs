using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public GameObject cam;
    public Vector3 offset;
    void Start()
    {
        
    }

    void Update()
    {
        cam.transform.position = new Vector3(transform.position.x + offset.x, transform.position.y + offset.y, -10 + offset.z);
    }
}
