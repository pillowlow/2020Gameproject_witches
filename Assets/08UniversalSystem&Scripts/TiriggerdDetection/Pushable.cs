using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]

public class Pushable : MonoBehaviour
{
    private Rigidbody2D rig;
    // Start is called before the first frame update
    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        GameObject player = col.gameObject;
        if (player.CompareTag("Player"))
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (Input.GetKey(KeyCode.F))
            {
                movement.SetJump(false);
                float x = Input.GetAxis("Horizontal");
                if (Math.Abs(x) > 0.1)
                {
                    rig.velocity = new Vector2((x < 0) ? -Mathf.Pow(-x, 1.4f) : Mathf.Pow(x, 1.4f) * movement.walkSpeed, 0);
                }
                else
                {
                    rig.velocity = new Vector2(0,0);
                }
            }
            else
            {                
                movement.SetJump(true);
                rig.velocity = new Vector2(0,0);
            }
        }
    }
}
