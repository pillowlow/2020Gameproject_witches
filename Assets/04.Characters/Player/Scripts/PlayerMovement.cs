using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
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

    public event Action OnJump;
    public event Action OnLanding;

    private bool _isMoveAble = true;
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
        Fall();
        Movement();
        if(PlayerManager.mode == PlayerManager.ModeCode.normal)
        {
            Jump();
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

        if (_isMoveAble && Mathf.Abs(x) > 0.1f)
        {
            Vector3 theScale = transform.localScale;

            if (x > 0 && theScale.x < 0 || x < 0 && theScale.x > 0)
            {
                theScale.x *= -1;
                transform.localScale = theScale;
            }

            rig.velocity = new Vector2(x * walkSpeed, rig.velocity.y);

            if (PlayerManager.state == PlayerManager.StateCode.Idle)
                PlayerManager.state = PlayerManager.StateCode.Moving;
        }
        else
        {
            if (PlayerManager.state == PlayerManager.StateCode.Moving)
                PlayerManager.state = PlayerManager.StateCode.Idle;
            
            rig.velocity = new Vector2(0, rig.velocity.y);
        }
    }

    void Jump()
    {
        if (_isMoveAble && PlayerManager.onGround && Input.GetButtonDown("Jump"))
        {    
            OnJump?.Invoke();
            rig.AddForce(new Vector2(0f, jumpForce));
            PlayerManager.state = PlayerManager.StateCode.Jumping;
        }
    }

    void Fall()
    {
        if(PlayerManager.state == PlayerManager.StateCode.Stop) return;
        if (PlayerManager.state == PlayerManager.StateCode.Jumping || PlayerManager.state == PlayerManager.StateCode.Flying)
        {
            if (rig.velocity.y < 0) PlayerManager.state = PlayerManager.StateCode.Falling;
        }
        else if (!PlayerManager.onGround && rig.velocity.y <= 0 && PlayerManager.state!=PlayerManager.StateCode.Die)
        {
            PlayerManager.state = PlayerManager.StateCode.Falling;
        }

        if(PlayerManager.state == PlayerManager.StateCode.Falling && PlayerManager.onGround)
        {
            PlayerManager.state = PlayerManager.StateCode.Idle;
            OnLanding?.Invoke();
        }
    }

    void Fly()
    {
        if (_isMoveAble && Input.GetButtonDown("Jump"))
         {
            rig.velocity = new Vector2(rig.velocity.x, 0);
            rig.AddForce(new Vector2(0, flyForce));
            PlayerManager.state = PlayerManager.StateCode.Flying;
         }

    }

    void CheckMoveable()
    {
        switch (PlayerManager.state)
        {
            case PlayerManager.StateCode.Die:
            case PlayerManager.StateCode.Stop:
                _isMoveAble = false;
                break;
            default:
                _isMoveAble = true;
                break;
        }
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
    void TakeHitEnd(){
        
    }
    
    void AnimationControl()
    {
        anim.SetFloat("SpeedX", Mathf.Abs(rig.velocity.x));
        anim.SetFloat("SpeedY", rig.velocity.y);
        anim.SetBool("OnGround", PlayerManager.onGround);
    }
}
