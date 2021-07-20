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
    public InputManager input;
    public float walkSpeed = 2;
    public float runSpeed = 5;
    public float flyForce = 35;
    public bool SprintToggle = true;
    public AnimationCurve AcceleratingCurve;    //Curve that describes how the character accelerates

    [Header("Jump")]
    public float jumpForce;
    public LayerMask groundLayer;

    public event Action OnJump;
    public event Action OnLanding;
    
    private bool _isJumpAble = true;
    private bool _isMoveable = true;
    private bool isHandle = false;
    private bool isSprinting = false;
    private Rigidbody2D rig;


    bool orient = false;                        //True means the player is facing right.False means the player is facing left.
    Vector3 Scale;                              //The scale of the main character.
    float WalkVelocityScaler(float x)           //It's the function that describes the relationship between the horizontal input and x-velocity.
    {
        return (x < 0) ? -AcceleratingCurve.Evaluate(-x) : AcceleratingCurve.Evaluate(x);
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
        ActionControl();
    }

    float _x_axis_value = 0;//internal static value for X_Axis function
    float X_Axis()//Return smooth horizontal input
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

    void Movement()//Calculate her speed and detect braking state
    {
        bool moving = !(PlayerManager.state == PlayerManager.StateCode.Jumping || PlayerManager.state == PlayerManager.StateCode.Falling);
        
        if(moving)
        {
            if (_isMoveable)
            {
                float x = X_Axis();
                float speed = Mathf.Abs(x);

                //Update isSprinting
                if (SprintToggle)
                {
                    if (input.GetKeyDown(InputAction.Sprint))
                    {
                        isSprinting = !isSprinting;
                    }
                }
                else
                {
                    isSprinting = input.GetKey(InputAction.Sprint);
                }

                if (speed > 0.1)
                {
                    if (isSprinting && !isHandle)
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
                    isSprinting = false;
                }
            }

            if (Mathf.Abs(rig.velocity.x) < 2.4f && PlayerManager.state == PlayerManager.StateCode.Running)
            {
                Brake();
            }
        }
    }
    
    void Brake()//Start to brake until her speed is 0
    {
        _isMoveable = false;
        isSprinting = false;
        PlayerManager.state = PlayerManager.StateCode.Braking;
        StartCoroutine(nameof(_Brake));
    }
    IEnumerator _Brake()//Brake coroutine function
    {
        //Replace Braking Animation Here
        portrait.CrossFade("Handle", 0.1f, 0);
        //Replace Braking Animation Here
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

    void Jump()//Jumping state detection and animation
    {
        if (input.GetKeyDown(InputAction.Jump) && _isMoveable)
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

    void Fall()//Falling state detection and animation
    {
        if(rig.velocity.y < -4 && !PlayerManager.onGround )
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
    void ActionControl()
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

        //Movement animation
        if (PlayerManager.state == PlayerManager.StateCode.Idle)
        {
            if (speed > 0.5f)
            {
                portrait.CrossFade("Walk", 0.3f, 0);
                PlayerManager.state = PlayerManager.StateCode.Walking;
            }
        }
        else if (PlayerManager.state == PlayerManager.StateCode.Walking && speed > 2.5f)
        {
            portrait.CrossFade("Run", 0.2f, 0);
            PlayerManager.state = PlayerManager.StateCode.Running;
        }
        else if (speed == 0 && (PlayerManager.state == PlayerManager.StateCode.Walking || PlayerManager.state == PlayerManager.StateCode.Running))
        {
            portrait.CrossFade("Idle", 0.2f, 0);
            PlayerManager.state = PlayerManager.StateCode.Idle;
        }

        //Interaction Animation
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


        //Reset _isMoveable to true
        if (PlayerManager.state != PlayerManager.StateCode.Braking)
        {
            _isMoveable = true;
        }
    }
}