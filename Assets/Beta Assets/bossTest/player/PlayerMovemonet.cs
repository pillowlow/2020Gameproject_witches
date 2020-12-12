using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovemonet : MonoBehaviour
{
    public float moveVelocity;

    Rigidbody2D rig;
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        if(Mathf.Abs(x) > 0.1)
        rig.velocity = new Vector2(x * moveVelocity, rig.velocity.y);
    }
}
