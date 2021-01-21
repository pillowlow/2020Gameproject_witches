using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Movement : MonoBehaviour
{
    [Header("Mode")]
    public PlayerManager.ModeCode initialMode;

    [Header("Movement")]
    public float walkSpeed;
    public float flyForce;

    [Header("Jump")]
    public float jumpForce;
    public LayerMask groundLayer;
    public Transform groundCheck;

    Rigidbody2D rig;
    Animator anim;

    float onGroundRadius = .05f;

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        PlayerManager.mode = initialMode;
    }

    void Update()
    {
        CheckOnGround();
        CheckMoveable();

        Movement();

        if(PlayerManager.mode == PlayerManager.ModeCode.normal)
        {
            Jump();
        }
        else if(PlayerManager.mode == PlayerManager.ModeCode.transform)
        {
            Fly();
        }

        Fall();

        AnimationControl();
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");

        if (PlayerManager.moveable && Mathf.Abs(x) > 0.1f)
        {
            Vector3 theScale = transform.localScale;

            if (x > 0 && theScale.x < 0)
            {
                theScale.x *= -1;
                transform.localScale = theScale;
            }
            else if (x < 0 && theScale.x > 0)
            {
                theScale.x *= -1;
                transform.localScale = theScale;
            }

            rig.velocity = new Vector2(x * walkSpeed, rig.velocity.y);

            if (PlayerManager.state == PlayerManager.StateCode.idle)
                PlayerManager.state = PlayerManager.StateCode.moving;
        }
        else
        {
            if (PlayerManager.state == PlayerManager.StateCode.moving)
                PlayerManager.state = PlayerManager.StateCode.idle;
            
            rig.velocity = new Vector2(0, rig.velocity.y);
        }
    }

    void Jump()
    {
        if (PlayerManager.moveable && PlayerManager.onGround && Input.GetButtonDown("Jump"))
        {
            rig.AddForce(new Vector2(0f, jumpForce));
            PlayerManager.state = PlayerManager.StateCode.jumping;
        }
    }

    void Fall()
    {
        if (PlayerManager.state == PlayerManager.StateCode.jumping || PlayerManager.state == PlayerManager.StateCode.flying)
        {
            if (rig.velocity.y < 0) PlayerManager.state = PlayerManager.StateCode.falling;
        }
        else if (!PlayerManager.onGround && rig.velocity.y <= 0)
        {
            PlayerManager.state = PlayerManager.StateCode.falling;
        }

        if(PlayerManager.state == PlayerManager.StateCode.falling && PlayerManager.onGround)
        {
            PlayerManager.state = PlayerManager.StateCode.idle;
        }
    }

    void Fly()
    {
        if (PlayerManager.moveable && Input.GetButtonDown("Jump"))
         {
            rig.velocity = new Vector2(rig.velocity.x, 0);
            rig.AddForce(new Vector2(0, flyForce));
            PlayerManager.state = PlayerManager.StateCode.flying;
         }

    }

    void CheckMoveable()
    {
        bool moveable = true;
        if (PlayerManager.state == PlayerManager.StateCode.takingHit) moveable = false;
        if (PlayerManager.state == PlayerManager.StateCode.die) moveable = false;
        if (PlayerManager.state == PlayerManager.StateCode.attack1) moveable = false;
        if (PlayerManager.state == PlayerManager.StateCode.attack1_connection) moveable = false;
        if (PlayerManager.state == PlayerManager.StateCode.attack2) moveable = false;
        if (PlayerManager.state == PlayerManager.StateCode.attack2_connection) moveable = false;

        PlayerManager.moveable = moveable;
    }

    void CheckOnGround()
    {
        PlayerManager.onGround = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, onGroundRadius, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                PlayerManager.onGround = true;
            }
        }
    }

    void AnimationControl()
    {
        anim.SetFloat("SpeedX", Mathf.Abs(rig.velocity.x));
        anim.SetFloat("SpeedY", rig.velocity.y);
        anim.SetBool("OnGround", PlayerManager.onGround);
    }
}
