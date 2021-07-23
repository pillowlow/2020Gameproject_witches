using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnyPortrait;
using UnityEditor;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;
    public GameObject Bottom;
    public apPortrait portrait;
    [Header("Movement")]
    private InputManager input;
    public float walkSpeed = 2;
    public float runSpeed = 5;
    public float flyForce = 35;
    public bool SprintToggle = true;
    public AnimationCurve AcceleratingCurve;    //Curve that describes how the character accelerates

    [Header("Jump")]
    public float jumpForce;
    public float lowJump = 1;
    public LayerMask groundLayer;

    public event Action OnJump;
    public event Action OnLanding;
    
    private bool _isJumpAble = true;
    private bool _isMoveable = true;
    public bool isHandle = false;
    public bool isSprinting { get; private set; } = false;
    private Rigidbody2D rig;
    public float SprintingSpeed = 5.5f;
    public float BrakingSpeed = 7.5f;

    private float capsuleRadius;
    private bool isFullSpeed = false;

    private bool orient = false;                        //True means the player is facing right.False means the player is facing left.
    Vector3 Scale;                              //The scale of the main character.
    float WalkVelocityScaler(float x)           //It's the function that describes the relationship between the horizontal input and x-velocity.
    {
        return (x < 0) ? -AcceleratingCurve.Evaluate(-x) : AcceleratingCurve.Evaluate(x);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            rig = GetComponent<Rigidbody2D>();
            Scale = transform.localScale;
            input = PlayerManager.instance.input;
            capsuleRadius = GetComponent<CapsuleCollider2D>().size.y / 4.0f;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.D))
        {
            int trap = 0;
        }
        Fall();
        Movement();
        Jump();
        ActionControl();
    }

    float _x_axis_value = 0;
    void X_Axis()//Update smooth horizontal input
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
    }

    void Movement()//Calculate her speed and detect braking state
    {
        bool moving = !(PlayerManager.state == PlayerManager.StateCode.Jumping || PlayerManager.state == PlayerManager.StateCode.Falling);
        X_Axis();
        if (moving)
        {
            if (_isMoveable)
            {
                float input_speed = Mathf.Abs(_x_axis_value);
                float rig_speed = Mathf.Abs(rig.velocity.x);
                //Detect Braking State
                if (isFullSpeed && rig_speed < BrakingSpeed && PlayerManager.state == PlayerManager.StateCode.Running)
                {
                    Brake();
                    isFullSpeed = false;
                    return;
                }

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

                if (input_speed == 0 && rig_speed < 0.1)
                {
                    rig.velocity = new Vector2(0, rig.velocity.y);
                    isSprinting = false;
                }
                else
                {
                    if (isSprinting && !isHandle)
                    {
                        rig_speed = WalkVelocityScaler(_x_axis_value) * runSpeed;
                        if (Mathf.Abs(rig.velocity.x) > BrakingSpeed)
                        {
                            isFullSpeed = true;
                        }
                    }
                    else
                    {
                        rig_speed = WalkVelocityScaler(_x_axis_value) * walkSpeed;
                    }
                    rig.AddForce(new Vector2(16 * (rig_speed - rig.velocity.x), 0));
                }
            }
        }

        //Fixed foot to the ground
        if (rig.velocity.y > 0 && PlayerManager.state != PlayerManager.StateCode.Jumping)
        {
            rig.velocity = new Vector2(rig.velocity.x, 0);
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

        if (rig.velocity.x > 0)
        {
            while (rig.velocity.x > 0.1f)
            {
                rig.AddForce(new Vector2(-rig.velocity.x * 16, 0));
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (rig.velocity.x < -0.1f)
            {
                rig.AddForce(new Vector2(-rig.velocity.x * 16, 0));
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
        if(rig.velocity.y > 0 && input.GetKey(InputAction.Jump))
        {
            rig.velocity += Vector2.up * lowJump;
        }
    }

    void Fall()//Falling state detection and animation
    {
        if (rig.velocity.y < -4 && !PlayerManager.onGround)
        {
            PlayerManager.state = PlayerManager.StateCode.Falling;
        }
        else if ((PlayerManager.state == PlayerManager.StateCode.Falling || (PlayerManager.state == PlayerManager.StateCode.Jumping && rig.velocity.y < 0.1)) && PlayerManager.onGround)
        {
            float speed = Mathf.Abs(rig.velocity.x);
            if (speed == 0)
            {
                portrait.CrossFade("Land", 0.2f);
                portrait.CrossFadeQueued("Idle", 0.2f);
                PlayerManager.state = PlayerManager.StateCode.Idle;
                isFullSpeed = false;
            }
            else if(speed < SprintingSpeed)
            {
                portrait.CrossFade("Walk", 0.2f);
                PlayerManager.state = PlayerManager.StateCode.Walking;
                isFullSpeed = false;
            }
            else
            {
                portrait.CrossFade("Run", 0.2f);
                PlayerManager.state = PlayerManager.StateCode.Running;
            }
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
            StartCoroutine(nameof(_CollisionExitHelper));
        }
    }
    IEnumerator _CollisionExitHelper()
    {
        yield return new WaitForSeconds(0.01f);
        
        RaycastHit2D rayHit= Physics2D.Raycast(Bottom.transform.position, Vector2.down, 0.1f, groundLayer);
        if (rayHit.collider == null)
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
            if (vx > 0.1f && (!orient))
            {
                gameObject.transform.localScale = new Vector3(-Scale.x, Scale.y, Scale.z);
                orient = true;
            }
            else if (vx < -0.1f && orient)
            {
                gameObject.transform.localScale = new Vector3(Scale.x, Scale.y, Scale.z);
                orient = false;
            }
        }

        //Movement animation
        if ((PlayerManager.state == PlayerManager.StateCode.Idle)|| (!isFullSpeed && PlayerManager.state == PlayerManager.StateCode.Running && speed < SprintingSpeed))
        {
            if (speed > 0.2f)
            {
                portrait.CrossFade("Walk", 0.3f, 0);
                PlayerManager.state = PlayerManager.StateCode.Walking;
            }
        }
        else if (PlayerManager.state == PlayerManager.StateCode.Walking && speed > SprintingSpeed)
        {
            portrait.CrossFade("Run", 0.2f, 0);
            PlayerManager.state = PlayerManager.StateCode.Running;
        }
        else if (speed == 0 && PlayerManager.state == PlayerManager.StateCode.Walking)
        {
            portrait.CrossFade("Idle", 0.2f, 0);
            PlayerManager.state = PlayerManager.StateCode.Idle;
        }

        //Interaction Animation
        //if (input.GetKeyDown(InputAction.Interact))
        //{
        //    if(isHandle)
        //    {
        //        portrait.CrossFade("Put", 0.1f, 1, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
        //        isHandle = false;
        //    }
        //    else
        //    {
        //        portrait.CrossFade("Take", 0.5f, 1);
        //        portrait.CrossFadeQueued("Handle", 0.4f, 1);
        //        isHandle = true;
        //    }
        //}


        //Reset _isMoveable to true
        if (PlayerManager.state != PlayerManager.StateCode.Braking)
        {
            _isMoveable = true;
        }
    }
}