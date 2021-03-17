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

    [Header("AnimationConfiguration")]
    [Range(0.0f, 5.0f)]
    public float BeginToWalk_Delay = 0.2f;      //The amount of time for starting to walk
    [Range(0.0f, 5.0f)]
    public float BeginToWalk_TimeScale = 1.6f;  //The time scale at the beginning of starting to walk
    [Range(0.0f, 5.0f)]
    public float Walking = 1.0f;                //The time scale among walking
    [Range(0.0f, 5.0f)]
    public float Jumping = 1.0f;                //The time scale of jumping animation
    [Range(0.0f, 5.0f)]
    public float RunJumping = 1.0f;             //The time scale of run-jumping animation
    [Range(0.0f, 5.0f)]
    public float JumpEnd = 0.5f;                //The end of the repeating (seconds)
    [Range(0.0f, 1.0f)]
    public float JumpRepeatSpeed = 0.5f;        //The time scale among repeating
    [Range(0.0f, 5.0f)]
    public float JumpRepeatTime = 0.3f;         //The amount of repeating time (seconds)
    [Range(0.0f, 5.0f)]
    public float RunJumpEnd = 0.5f;             //same thing but for run-jump
    [Range(0.0f, 1.0f)]
    public float RunJumpRepeatSpeed = 0.5f;     //same thing but for run-jump
    [Range(0.0f, 5.0f)]
    public float RunJumpTime = 0.3f;            //same thing but for run-jump
    public float p = 0.4f;                      //The constant to predict whether to play animation or not when falling (It's for optimization.It's needed because playing two animation in a short period will cause lag)
    
    AnimationControl ac;                        //Handling everything about DragonBones
    Rigidbody2D rig;


    public event Action OnJump;
    public event Action OnLanding;

    private bool _isMoveAble = true;

    
    int IdleCounter = 0;                        //When player doesn't move for 30 frames, we play idle animation. (same reason as p variable. it's for optimization.)
    bool orient = false;                        //True means the player is facing right.False means the player is facing left.
    Vector3 Scale;                              //The scale of the main character.
    PlayerAttack playerAttack;
    bool BeginningOfWalking = false;            //True means now is the beginning of starting to walk.
    float WalkVelocityScaler(float x)           //It's the math function to describe the relationship between the horizontal input and x-velocity. (It can be replaced by a better function.)
    {
        float sign = 1;
        if (x < 0) { sign = -1; x = -x; }
        x = Mathf.Pow(x, 1.4f);
        return x * sign;
    }
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        PlayerManager.mode = initialMode;
        Scale = transform.localScale;
        ac = new AnimationControl(GetComponent<DragonBones.UnityArmatureComponent>());
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
            if (Mathf.Abs(x) > 0.1 && _isMoveAble)
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
        if (_isMoveAble && PlayerManager.onGround && Input.GetButtonDown("Jump"))
        {    
            OnJump?.Invoke();
            if (PlayerManager.state == PlayerManager.StateCode.Idle)
            {
                ac.jump.SetTimeScale(Jumping);
                ac.jump.Play(-1,false,0.2f, () => { rig.AddForce(new Vector2(0f, jumpForce)); },this);
            }
            else
            {
                ac.runjump.SetTimeScale(RunJumping);
                ac.runjump.Play(-1, false, 0.2f, () => { rig.AddForce(new Vector2(0f, jumpForce)); }, this);
            }
            PlayerManager.state = PlayerManager.StateCode.Jumping;
        }
    }

    void Fall()
    {
        if(rig.velocity.y < 0 && !PlayerManager.onGround )
        {
            PlayerManager.state = PlayerManager.StateCode.Falling;
            if(rig.velocity.y < -1.5f && ac.Playing()!=ac.falling)
            {
                /*Animation Parameter Setting*/
                ac.falling.JumpEnd = JumpEnd;
                ac.falling.JumpRepeatSpeed = JumpRepeatSpeed;
                ac.falling.JumpRepeatTime = JumpRepeatTime;
                ac.falling.RunJumpEnd = RunJumpEnd;
                ac.falling.RunJumpRepeatSpeed = RunJumpRepeatSpeed;
                ac.falling.RunJumpTime = RunJumpTime;
                /*Animation Parameter Setting*/
                Collider2D overlap =  Physics2D.OverlapPoint(new Vector2(transform.position.x + p * rig.velocity.x, (transform.position.y - 1) + p * rig.velocity.y + 0.5f * Physics2D.gravity.y * p*p), groundLayer);
                if(overlap == null)
                {
                    ac.falling.fall(this);
                }
            }
            
        }
        else if(PlayerManager.state == PlayerManager.StateCode.Falling && PlayerManager.onGround)
        {
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
    void TakeHitEnd(){
        
    }
    
    void AnimationControl()
    {
        float vx = rig.velocity.x;              //x of velocity
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
        if (PlayerManager.state == PlayerManager.StateCode.Idle || PlayerManager.state == PlayerManager.StateCode.Moving)
        {
            if (Mathf.Abs(vx) > 0.5f)
            {
                //If it is the first frame of running animation, then play "walk(run)start"
                if (ac.Playing()!=ac.run && ac.Playing()!=ac.walk)
                {
                    BeginningOfWalking = true;
                    ac.walk.StartWalking(this,BeginToWalk_Delay, () => { BeginningOfWalking = false; });
                }
                if (BeginningOfWalking)
                {
                    ac.walk.SetTimeScale(walkSpeed * BeginToWalk_TimeScale);
                }
                else
                {
                    ac.run.SetTimeScale(Mathf.Abs(vx)*Walking);
                }

                PlayerManager.state = PlayerManager.StateCode.Moving;
            }
            //If character stopped running, then start to count up "IdleCounter"
            else if (ac.Playing()==ac.walk || ac.Playing()==ac.run)
            {
                IdleCounter++;
                //If "IdleCounter" counts up to 30, then we assume player stopped to idle
                if (IdleCounter > 30)
                {
                    ac.idle.Play(0.1f,true);
                    IdleCounter = 0;
                    PlayerManager.state = PlayerManager.StateCode.Idle;
                }
            }
            else
            {
                IdleCounter = 0;
            }
        }
        //Do some check for optimizing performance
        if (AnimationContCheck())
        {
            //If character is not moving
            if (Mathf.Abs(ix) < 0.2f)
            {
                PlayerManager.state = PlayerManager.StateCode.Idle;
                ac.idle.Play(0.2f,true);
            }
            //If character is still moving
            else
            {
                PlayerManager.state = PlayerManager.StateCode.Moving;
            }
        }
    }

    bool AnimationContCheck()
    {
        switch (PlayerManager.state)
        {
            case PlayerManager.StateCode.Falling: { if (PlayerManager.onGround) { return true; } break; }
            case PlayerManager.StateCode.TakingHit:
            case PlayerManager.StateCode.Reborn:
            {
                if (ac.isCompleted()) { return true; } break; 
            }
            case PlayerManager.StateCode.Jumping:
            {
                if((ac.Playing()==ac.jump||ac.Playing()==ac.runjump)&&ac.isCompleted())
                {
                    return true;
                }
                break;
            }
        }

        switch(playerAttack._attackMode)
        {
            case PlayerAttack.AttackMode.attack1:
            case PlayerAttack.AttackMode.attack2:
            {
                if (ac.isCompleted()) { return true; }
                break;
            }
        }
        return false;
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
