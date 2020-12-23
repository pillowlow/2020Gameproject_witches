using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiranhaPatrol : MonoBehaviour
{
    private Rigidbody2D Rigidbody2D;
    public GameObject leftPoint,rightPoint;
    private float left, right;
    public float speed;
    private bool isRight=true;
    
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        left = leftPoint.transform.position.x;
        right = rightPoint.transform.position.x;
        Destroy(leftPoint);
        Destroy(rightPoint);
    }

    // Update is called once per frame
    void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        if (isRight)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            Rigidbody2D.velocity=new Vector2(speed,Rigidbody2D.velocity.y);
            if (transform.position.x > right)
            {
                isRight = false;
            }
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            Rigidbody2D.velocity=new Vector2(-speed,Rigidbody2D.velocity.y);
            if (transform.position.x < left) ;
            {
                isRight = true;
            }
        }
    }
}
