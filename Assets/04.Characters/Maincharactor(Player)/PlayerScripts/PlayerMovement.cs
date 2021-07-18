using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnyPortrait;
public class PlayerMovement : MonoBehaviour
{
    public apPortrait portrait;
    [Header("Mode")]
    public PlayerManager.ModeCode initialMode;

    [Header("Movement")]
    public float walkSpeed;
    public float flyForce = 1;

    [Header("Jump")]
    public float jumpForce;
    public LayerMask groundLayer;


    public event Action OnJump;
    public event Action OnLanding;

    private bool _isJumpAble = true;
    private Rigidbody2D rig;

    bool orient = false;                        //True means the player is facing right.False means the player is facing left.
    Vector3 Scale;                              //The scale of the main character.
    PlayerAttack playerAttack;
    float WalkVelocityScaler(float x)           //It's the math function to describe the relationship between the horizontal input and x-velocity. (It can be replaced by a better function.)
    {
        return (x < 0) ? -Mathf.Pow(-x, 1.4f) : Mathf.Pow(x, 1.4f);
    }
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        PlayerManager.mode = initialMode;
        Scale = transform.localScale;
        playerAttack = GetComponent<PlayerAttack>();
    }

    void Update()
    {
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
        if (Controlable())
        {
            if (Mathf.Abs(x) > 0.1)
            {
                rig.velocity = new Vector2(WalkVelocityScaler(x) * walkSpeed, rig.velocity.y);
            }
            else
            {
                rig.velocity = new Vector2(0, rig.velocity.y);
            }
        }
    }
    

    void Jump()
    {
        if(Input.GetButtonDown("Jump"))
        {
            if (PlayerManager.onGround && _isJumpAble)
            {
                OnJump?.Invoke();
                portrait.CrossFade("Jump", 0.1f);
                rig.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                portrait.CrossFade("Fall", 0.1f);
                PlayerManager.state = PlayerManager.StateCode.Jumping;
            }

        }
        
    }

    void Fall()
    {
        if(rig.velocity.y < 0 && !PlayerManager.onGround )
        {
            PlayerManager.state = PlayerManager.StateCode.Falling;
        }
        else if(PlayerManager.state == PlayerManager.StateCode.Falling && PlayerManager.onGround)
        {
            portrait.CrossFade("Idle", 0.2f);
            PlayerManager.state = PlayerManager.StateCode.Idle;
            OnLanding?.Invoke();
        }
    }

    void Fly()
    {
        if (Input.GetButtonDown("Jump"))
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
                //_isMoveAble = false;
                break;
            default:
                //_isMoveAble = true;
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            PlayerManager.onGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            PlayerManager.onGround = false;
        }
    }

    public void SetJump(bool jumpAble)
    {
        _isJumpAble = jumpAble;
    }
    void AnimationControl()
    {
        float vx = rig.velocity.x;              //x of velocity
        float speed = Mathf.Abs(vx);
        float ix = Input.GetAxis("Horizontal"); //x of input
        //Character orientation check
        if (Controlable())
        {
            if (vx > 0 && (!orient))
            {
                gameObject.transform.localScale = new Vector3(-Scale.x, Scale.y, Scale.z);
                orient = true;
            }
            else if (vx < 0 && orient)
            {
                gameObject.transform.localScale = new Vector3(Scale.x, Scale.y, Scale.z);
                orient = false;
            }
        }
        //Horizontal movement animation
        //Movement animation played if character state is "idle" or "moving"
        if (PlayerManager.state == PlayerManager.StateCode.Idle)
        {
            if (speed > 0.5f)
            {
                portrait.CrossFade("Walk", 0.3f);
                //portrait.CrossFade("Run", 0.2f);
                PlayerManager.state = PlayerManager.StateCode.Moving;
            }
        }
        else if (speed == 0 && PlayerManager.state == PlayerManager.StateCode.Moving)
        {
            portrait.CrossFade("Idle", 0.2f);
            PlayerManager.state = PlayerManager.StateCode.Idle;
        }

    }

    
    bool Controlable()
    {
        if (
            PlayerManager.state == PlayerManager.StateCode.TakingHit ||
            PlayerManager.state == PlayerManager.StateCode.Die
            )
        {
            return false;
        }
        return true;
    }

    
}
