using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class player : MonoBehaviour
{
    public float speed = 0.1f;
    public Rigidbody2D mario;
    public float xspeed_limit = 10;

    // Start is called before the first frame update
    void Start()
    {
        print("start");
        mario = this.gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            mario.AddForce(new Vector2(0.5f, 0), ForceMode2D.Impulse);
            //this.gameObject.transform.position += new Vector3(speed, 0, 0);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            mario.AddForce(new Vector2(-0.5f, 0), ForceMode2D.Impulse);
            //this.gameObject.transform.position += new Vector3((-1)*speed, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            mario.AddForce(new Vector2(0, 12), ForceMode2D.Impulse);
            print("jump");
        }
        if (mario.velocity.x > xspeed_limit)
        { mario.velocity = new Vector2(xspeed_limit, mario.velocity.y); }
        if (mario.velocity.x < -xspeed_limit)
        { mario.velocity = new Vector2(-xspeed_limit, mario.velocity.y); }
    }
}
