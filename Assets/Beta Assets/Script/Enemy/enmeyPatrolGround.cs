using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enmeyPatrolGround : MonoBehaviour
{
    public float speed;
    private float distance=0.5f;
    
    private bool isRight=true;
    public Transform groundDetection;
    
    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right*speed*Time.deltaTime);

        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down,distance );
        if (groundInfo.collider == false)
        {
            if (isRight == true)
            {
                transform.eulerAngles=new Vector3(0,-180,0);
                isRight = false;
            }
            else
            {
                transform.eulerAngles=new Vector3(0,0,0);
                isRight = true;
            }
        }
    }
}
