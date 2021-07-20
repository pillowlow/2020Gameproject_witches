using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnyPortrait;
using UnityEditor;

public class PlayerMovement : MonoBehaviour
{
    public apPortrait portrait;
    [Header("Mode")]
    public PlayerManager.ModeCode initialMode;

    [Header("Movement")]
    public float walkSpeed = 2;
    public float runSpeed = 5;
    public float flyForce = 1;

    [Header("Jump")]
    public float jumpForce;
    public LayerMask groundLayer;

    public event Action OnJump;
    public event Action OnLanding;
    public InputManager input;
    private bool _isJumpAble = true;
    public bool _isMoveable = true;
    private bool isHandle = false;
    private Rigidbody2D rig;


    bool orient = false;                        //True means the player is facing right.False means the player is facing left.
    Vector3 Scale;                              //The scale of the main character.
    float WalkVelocityScaler(float x)           //It's the math function to describe the relationship between the horizontal input and x-velocity. (It can be replaced by a better function.)
    {
        return (x < 0) ? -Mathf.Pow(-x, 1.4f) : Mathf.Pow(x, 1.4f);
    }
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        PlayerManager.mode = initialMode;
        Scale = transform.localScale;
    }

    void Update()
    {
        Fall();
        Movement();
        Jump();
        AnimationControl();
        if (PlayerManager.state != PlayerManager.StateCode.Braking)
        {
            _isMoveable = true;
        }
    }

    float _x_axis_value = 0;
    float X_Axis()
    {
        float speed = 2;
        if (input.GetKey(InputAction.Right))
        {
            _x_axis_value = _x_axis_value < 1 ? _x_axis_value + speed * Time.deltaTime : 1;
        }
        else if (input.GetKey(InputAction.Left))
        {
            _x_axis_value = _x_axis_value > -1 ? _x_axis_value - speed * Time.deltaTime : -1;
        }
        else if (_x_axis_value != 0)
        {
            float tem_x = _x_axis_value;
            _x_axis_value = _x_axis_value > 0 ? _x_axis_value - speed * Time.deltaTime : _x_axis_value + speed * Time.deltaTime;
            if ((tem_x > 0 && _x_axis_value < 0) || (tem_x < 0 && _x_axis_value > 0))
            {
                _x_axis_value = 0;
            }
        }
        return _x_axis_value;
    }

    void Movement()
    {
        float x = X_Axis();
        float speed = Mathf.Abs(x);
        bool moving = !(PlayerManager.state == PlayerManager.StateCode.Jumping || PlayerManager.state == PlayerManager.StateCode.Falling);
        if (_isMoveable && moving)
        {
            if (speed > 0.1)
            {
                if (input.GetKey(InputAction.Sprint) && !isHandle)
                {
                    rig.velocity = new Vector2(WalkVelocityScaler(x) * runSpeed, rig.velocity.y);
                }
                else
                {
                    rig.velocity = new Vector2(WalkVelocityScaler(x) * walkSpeed, rig.velocity.y);
                }
            }
            else
            {
                rig.velocity = new Vector2(0, rig.velocity.y);
            }
        }
        if (Mathf.Abs(rig.velocity.x) < 2.4f && PlayerManager.state == PlayerManager.StateCode.Running && moving)
        {
            Brake();
        }
    }
    
    void Brake()
    {
        _isMoveable = false;
        PlayerManager.state = PlayerManager.StateCode.Braking;
        StartCoroutine(nameof(_Brake));
    }
    IEnumerator _Brake()
    {
        //Braking Animation Here
        portrait.CrossFade("Handle", 0.1f, 0);
        //Braking Animation Here
        float brakingFactor = 1;
        
        float originalVelocity_x = rig.velocity.x;
        if (originalVelocity_x > 0)
        {
            while (originalVelocity_x > 0)
            {
                rig.velocity = new Vector2(originalVelocity_x -= brakingFactor * Time.deltaTime, rig.velocity.y);
           
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (originalVelocity_x < 0)
            {
                rig.velocity = new Vector2(originalVelocity_x += brakingFactor * Time.deltaTime, rig.velocity.y);
                yield return new WaitForEndOfFrame();
            }
        }
        rig.velocity = new Vector2(0, rig.velocity.y);
        PlayerManager.state = PlayerManager.StateCode.Idle;
        portrait.CrossFade("Idle", 0.3f, 0);
        _isMoveable = true;
    }

    void Jump()
    {
        if(input.GetKeyDown(InputAction.Jump))
        {
            if (PlayerManager.onGround && _isJumpAble && (!isHandle))
            {
                OnJump?.Invoke();
                portrait.CrossFade("Jump", 0.1f, 0);
                rig.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                portrait.CrossFadeQueued("Fall", 0.1f, 0);

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
            portrait.CrossFade("Land", 0.2f);
            portrait.CrossFadeQueued("Idle", 0.2f);
            PlayerManager.state = PlayerManager.StateCode.Idle;
            OnLanding?.Invoke();
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
        float vx = rig.velocity.x;
        float speed = Mathf.Abs(vx);
        //Character orientation check
        if (_isMoveable)
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
                if (isHandle)
                {
                    portrait.CrossFade("Walk", 0.3f, 0);
                }
                else
                {
                    portrait.CrossFade("Walk", 0.3f, 0);
                }
                PlayerManager.state = PlayerManager.StateCode.Walking;
            }
        }
        else if (PlayerManager.state == PlayerManager.StateCode.Walking && speed > 2.5f)
        {
            portrait.CrossFade("Run", 0.2f, 0);
            PlayerManager.state = PlayerManager.StateCode.Running;
        }
        else if (speed == 0 && (PlayerManager.state == PlayerManager.StateCode.Walking|| PlayerManager.state == PlayerManager.StateCode.Running))
        {
            portrait.CrossFade("Idle", 0.2f, 0);
            PlayerManager.state = PlayerManager.StateCode.Idle;
        }

        if (input.GetKeyDown(InputAction.Interact))
        {
            if(isHandle)
            {
                portrait.CrossFade("Put", 0.1f, 1);
                isHandle = false;
            }
            else
            {
                portrait.CrossFade("Take", 0.5f, 1);
                portrait.CrossFadeQueued("Handle", 0.4f, 1);
                isHandle = true;
            }
        }
    }
}