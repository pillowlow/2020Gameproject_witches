using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Movement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed;
    public float flyForce;

    [Header("Jump")]
    public float jumpForce;
    public LayerMask groundLayer;
    public Transform groundCheck;

    Rigidbody2D rig;
    Animator anim;

    bool fly = false;

    float onGroundRadius = .05f;

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Movement();

        if(PlayerManager.mode == PlayerManager.ModeCode.normal)
        {
            Jump();
            Fall();
        }
        else if(PlayerManager.mode == PlayerManager.ModeCode.transform)
        {
            Fly();
        }

        AnimationControl();
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");

        CheckMoveable();

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

            if (PlayerManager.state == PlayerManager.StateCode.idel)
                PlayerManager.state = PlayerManager.StateCode.moving;
        }
        else
        {
            if (PlayerManager.state == PlayerManager.StateCode.moving)
                PlayerManager.state = PlayerManager.StateCode.idel;
            
            rig.velocity = new Vector2(0, rig.velocity.y);
        }
    }

    void Jump()
    {
        CheckOnGround();

        if (PlayerManager.moveable && PlayerManager.onGround && Input.GetButtonDown("Jump"))
        {
            rig.AddForce(new Vector2(0f, jumpForce));
            PlayerManager.state = PlayerManager.StateCode.jumping;
        }
    }

    void Fall()
    {
        if (PlayerManager.state == PlayerManager.StateCode.jumping)
        {
            if (rig.velocity.y < 0) PlayerManager.state = PlayerManager.StateCode.falling;
        }
        else if (!PlayerManager.onGround && rig.velocity.y <= 0)
        {
            PlayerManager.state = PlayerManager.StateCode.falling;
        }


        if(PlayerManager.state == PlayerManager.StateCode.falling && PlayerManager.onGround)
        {
            PlayerManager.state = PlayerManager.StateCode.idel;
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

        if (Input.GetKeyDown(KeyCode.J)) anim.SetTrigger("TakeHit");

        /*if (fly)
        {
            anim.SetTrigger("Fly");
            fly = false;
            Fly();
        }*/

    }
}
