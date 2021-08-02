using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnyPortrait;
using UnityEditor;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2;
    public float runSpeed = 5;
    public float pushingSpeed = 1;
    public bool SprintToggle = true;
    public float SprintingSpeed = 5.5f;
    public float BrakingSpeed = 7.5f;
    public float SpeedInAir = 2;
    public float ClimbOffset = 0.5f;
    public float chainnn = 10;
    public AnimationCurve AcceleratingCurve;    //Curve that describes how the character accelerates

    [Header("Jump")]
    public float jumpForce;
    public float highJump = 1;
    public float JumpForwardFactor = 1;
    public event Action OnJump;
    public event Action OnLanding;
    private bool _isJumpAble = true;
    private bool _isMoveable = true;
    private InputManager input;
    [Header("Layers")]
    public LayerMask groundLayer;
    [HideInInspector]
    public apPortrait portrait;
    public static PlayerMovement instance;
    public bool orient { get; private set; } = false;                        //True means the player is facing right.False means the player is facing left.

    public bool isSprinting { get; private set; } = false;
    public Rigidbody2D rig { get; private set; }
    private float capsuleRadius;
    private bool isFullSpeed = false;
    private bool isFirstFrame = true;
    Vector3 Scale;                                      //The scale of the main character.
    WaitForSeconds Wait100ms = new WaitForSeconds(0.1f);
    public enum Actions_Type{ cast, push, drag, port };
    bool ContinueBrake = true;
    const string Animation_Idle         = "Idle";
    const string Animation_Walk         = "Walk";
    const string Animation_Run          = "Run";
    const string Animation_Jump         = "Jump";
    const string Animation_Fall         = "Fall";
    const string Animation_Land         = "Land";
    const string Animation_Brake        = "Land";       //Replace this when we have the animation
    const string Animation_Push_Start   = "StartPush";
    const string Animation_Push         = "Push";
    const string Animation_Pick         = "Pick";
    const string Animation_Idle_Weapon  = "IdleWithWeapon";
    const string Animation_Climb_Start  = "StartClimb";
    const string Animation_Climb_Idle   = "StayOnRope";
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
            portrait = GetComponent<apPortrait>();
            Scale = transform.localScale;
            input = PlayerManager.instance.input;
            PlayerManager.state = PlayerManager.StateCode.Idle;
            capsuleRadius = GetComponent<CapsuleCollider2D>().size.y / 4.0f;
            StartCoroutine(nameof(_CollisionDetectionHelper));
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    void Update()
    {
        Movement();
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

    /*
        states : idle walk run brake crawl ride tic-tac knock float jump fall take action (cast push drag port)
        
        animation transitions table
        ----------------------------------------------------------
        idle 	-> walk crawl ride float knock take jump fall
        walk 	-> idle run crawl ride knock float take jump fall
        run 	-> brake jump fall
        brake 	-> idle fall
        crawl 	-> idle walk
        ride 	-> idle
        tic-tac -> idle fall
        knock 	-> idle
        float 	-> idle walk fall
        jump 	-> idle walk tic-tac float fall
        fall 	-> (land)idle walk float
        take	-> action
     */
    void Movement()//Calculate her speed and detect braking state
    {
        X_Axis();
        UpdateIsSprinting();
        switch (PlayerManager.state)
        {
            case PlayerManager.StateCode.Idle:
            {
                IdleState(false);
                break;
            }
            case PlayerManager.StateCode.Walk:
            {
                WalkState(false);
                break;
            }
            case PlayerManager.StateCode.Run:
            {
                RunState(false);
                break;
            }
            case PlayerManager.StateCode.Brake:
            {
                BrakeState(false);
                break;
            }
            case PlayerManager.StateCode.Jump:
            {
                JumpState(false);
                break;
            }
            case PlayerManager.StateCode.Fall:
            {
                FallState();
                break;
            }
            case PlayerManager.StateCode.Action_push:
            {
                PushState(false);
                break;
            }
            case PlayerManager.StateCode.Action_pick:
            {
                PickState(false);
                break;
            }
            case PlayerManager.StateCode.Climb:
            {
                ClimbState(false);
                break;
            }
        }

        if(Mathf.Abs(rig.velocity.x) < walkSpeed)
        {
            isFullSpeed = false;
        }

    }

    void BrakeState(bool transition)// ( completed )
    {
        PlayerManager.state = PlayerManager.StateCode.Brake;
        if (transition) { isFirstFrame = true; return; }


        /*------------Start of State Transitions------------*/
        //idle
        if (!(isFirstFrame || portrait.IsPlaying(Animation_Brake)))
        {
            IdleState(true);
            return;
        }
        //fall
        if(!PlayerManager.onGround && rig.velocity.y<-1)
        {
            ContinueBrake = false;
            FallState();
            return;
        }
        /*------------End of State Transitions------------*/


        _isMoveable = false;
        isSprinting = false;
        if(isFirstFrame)
        {
            isFirstFrame = false;
            portrait.CrossFade(Animation_Brake, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
            StartCoroutine(nameof(_Brake));
        }
       
    }

    IEnumerator _Brake()//Brake coroutine function
    {
        if (rig.velocity.x > 0)
        {
            while (rig.velocity.x > 0.1f && ContinueBrake)
            {
                rig.AddForce(new Vector2(-rig.velocity.x * 16, 0));
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (rig.velocity.x < -0.1f && ContinueBrake)
            {
                rig.AddForce(new Vector2(-rig.velocity.x * 16, 0));
                yield return new WaitForEndOfFrame();
            }
        }
        ContinueBrake = true;
        rig.velocity = new Vector2(0, rig.velocity.y);
        PlayerManager.state = PlayerManager.StateCode.Idle;
        _isMoveable = true;
    }

    void IdleState(bool transition)
    {
        PlayerManager.state = PlayerManager.StateCode.Idle;
        if (transition) { isFirstFrame = true; return; }

        /*------------Start of State Transitions------------*/
        //walk
        if (Mathf.Abs(_x_axis_value) > 0.1f)
        {
            WalkState(true);
            return;
        }
        //crawl
        //ride
        //float
        //knock
        //take
        {
            //Push
            if (Pushable.isPushing)
            {
                PushState(true);
                return;
            }
            //Pick
            if(Pickable.isTaking)
            {
                PickState(true);
                return;
            }
            //Throw
            if(PlayerManager.isTaking && input.GetKeyDown(InputAction.Interact))
            {
                Pickable.held.Throw(new Vector2(-1000,1000));
                PlayerManager.isTaking = false;
                IdleState(true);
                return;
            }
        }
        
        //jump
        if (input.GetKeyDown(InputAction.Jump) && _isMoveable)
        {
            JumpState(true);
            return;
        }
        //fall
        if (rig.velocity.y < 0 && !PlayerManager.onGround)
        {
            FallState();
            return;
        }
        /*------------End of State Transitions------------*/


        if (isFirstFrame)
        {
            portrait.CrossFade(PlayerManager.isTaking && Pickable.held.isWeapon ? Animation_Idle_Weapon : Animation_Idle);
            isFirstFrame = false;
        }
        isSprinting = false;
    }

    void WalkState(bool transition)
    {
        PlayerManager.state = PlayerManager.StateCode.Walk;
        if (transition) { isFirstFrame = true; return; }


        /*------------Start of State Transitions------------*/
        //idle
        if (Mathf.Abs(rig.velocity.x) < 0.1f && !input.isHorizonInput())
        {
            rig.velocity = new Vector2(0, rig.velocity.y);
            
            IdleState(true);
            return;
        }
        //run
        
        if(isSprinting)
        {
            RunState(true);
            return;
        }
        //crawl
        //ride
        //knock
        //float
        //take
        if (Pushable.isPushing)
        {
            PushState(true);
            return;
        }
        //jump
        if (input.GetKeyDown(InputAction.Jump) && _isMoveable)
        {
            JumpState(true);
            return;
        }
        //fall
        if (rig.velocity.y < 0 && !PlayerManager.onGround)
        {
            FallState();
            return;
        }
        /*------------End of State Transitions------------*/


        UpdateOrientation();
        float speed = WalkVelocityScaler(_x_axis_value) * walkSpeed;
        rig.AddForce(new Vector2(16 * (speed - rig.velocity.x), 0));

        if (isFirstFrame)
        {
            portrait.CrossFade(Animation_Walk);
            isFirstFrame = false;
        }
        isSprinting = false;
    }

    void RunState(bool transition)
    {
        PlayerManager.state = PlayerManager.StateCode.Run;
        if (transition) { isFirstFrame = true; return; }


        /*------------Start of State Transitions------------*/
        //Brake
        if (((Mathf.Abs(rig.velocity.x) < BrakingSpeed || !isSprinting) && isFullSpeed) || (Mathf.Abs(rig.velocity.x) < walkSpeed && Mathf.Abs(_x_axis_value) < 0.1f))
        {
            BrakeState(true);
            return;
        }
        //Jump
        if (input.GetKeyDown(InputAction.Jump) && _isMoveable)
        {
            JumpState(true);
            return;
        }
        //Fall
        if (rig.velocity.y < 0 && !PlayerManager.onGround)
        {
            FallState();
            return;
        }
        /*------------End of State Transitions------------*/


        float speed = WalkVelocityScaler(_x_axis_value) * runSpeed;
        rig.AddForce(new Vector2(16 * (speed - rig.velocity.x), 0));
        if (Mathf.Abs(rig.velocity.x) > BrakingSpeed)
        {
            isFullSpeed = true;
        }
        if (isFirstFrame)
        {
            portrait.CrossFade(Animation_Run);
            isFirstFrame = false;
        }
        if (Mathf.Abs(rig.velocity.x) < float.Epsilon)
        {
            isSprinting = false;
        }
        UpdateOrientation();
    }

    void JumpState(bool transition)//Jumping state detection and animation
    {
        PlayerManager.state = PlayerManager.StateCode.Jump;

        //Additional speed boost
        {
            //high jump
            if (rig.velocity.y > 0 && input.GetKey(InputAction.Jump))
            {
                rig.velocity += highJump * Vector2.up * Time.deltaTime;
            }

            //Horizontal movement

            if(input.GetKey(InputAction.Right) && rig.velocity.x < SpeedInAir && rig.velocity.x > -0.001f)
            {
                rig.AddForce(new Vector2(16 * (SpeedInAir - rig.velocity.x), 0));
            }
            else if(input.GetKey(InputAction.Left) && rig.velocity.x > -SpeedInAir && rig.velocity.x < 0.001f)
            {
                rig.AddForce(new Vector2(16 * (rig.velocity.x - SpeedInAir), 0));
            }
            
        }

        if (input.GetKeyDown(InputAction.Jump) && _isMoveable)
        {
            if (PlayerManager.onGround && _isJumpAble)
            {
                OnJump?.Invoke();
                rig.AddForce(new Vector2(rig.velocity.x * JumpForwardFactor, jumpForce), ForceMode2D.Impulse);
            }
        }
        if (transition) { isFirstFrame = true; return; }


        /*------------Start of State Transitions------------*/
        //idle
        if (Mathf.Abs(rig.velocity.x)<0.01f && Mathf.Abs(rig.velocity.y) < 0.01f && PlayerManager.onGround)
        {
            IdleState(true);
            return;
        }
        //walk
        if (Mathf.Abs(rig.velocity.y) < 0.01f && PlayerManager.onGround)
        {
            WalkState(true);
            return;
        }
        //tic - tac
        //float
        //fall
        if (rig.velocity.y < 0 && !PlayerManager.onGround)
        {
            FallState();
            return;
        }
        /*------------End of State Transitions------------*/

        UpdateOrientation();
        if (isFirstFrame)
        {
            portrait.CrossFade(Animation_Jump);
            portrait.CrossFadeQueued(Animation_Fall);
            isFirstFrame = false;
        }
        if(Mathf.Abs(rig.velocity.x) < walkSpeed)
        {
            isSprinting = false;
        }
    }

    void FallState()// ( completed )
    {
        PlayerManager.state = PlayerManager.StateCode.Fall;

        /*------------Start of State Transitions------------*/
        if (PlayerManager.onGround)
        {
            float speed = Mathf.Abs(rig.velocity.x);
            //land
            if (speed < 0.01f)
            {
                isFullSpeed = false;
                LandState();
            }
            //walk
            else if (speed < SprintingSpeed)
            {
                isFullSpeed = false;
                WalkState(true);
            }
            //run
            else
            {
                RunState(true);
            }
            OnLanding?.Invoke();
            return;
        }
        /*------------End of State Transitions------------*/

        //Horizontal movement

        if (input.GetKey(InputAction.Right) && rig.velocity.x < SpeedInAir && rig.velocity.x > -0.001f)
        {
            rig.AddForce(new Vector2(16 * (SpeedInAir - rig.velocity.x), 0));
        }
        else if (input.GetKey(InputAction.Left) && rig.velocity.x > -SpeedInAir && rig.velocity.x < 0.001f)
        {
            rig.AddForce(new Vector2(16 * (rig.velocity.x - SpeedInAir), 0));
        }
        UpdateOrientation();

        if (isFirstFrame)
        {
            portrait.CrossFade(Animation_Fall);
            isFirstFrame = false;
        }
        if (Mathf.Abs(rig.velocity.x) < walkSpeed)
        {
            isSprinting = false;
        }
    }

    void LandState()
    {
        portrait.CrossFade(Animation_Land);
        if(!portrait.IsPlaying(Animation_Land))
        {
            IdleState(true);
            return;
        }
    }

    void PushState(bool transition)
    {
        PlayerManager.state = PlayerManager.StateCode.Action_push;
        if (transition) {isFirstFrame = true; return; }
        /*------------Start of State Transitions------------*/
        if (!Pushable.isPushing)
        {
            IdleState(true);
            return;
        }
        if(Vector3.Dot(rig.velocity, Pushable.PushedObject.position - PlayerManager.instance.player.transform.position) < 0)
        {
            Pushable.isPushing = false;
            WalkState(true);
            return;
        }
        /*------------End of State Transitions------------*/
        if (isFirstFrame)
        {
            portrait.CrossFade(Animation_Push_Start);
            portrait.CrossFadeQueued(Animation_Push);
            isFirstFrame = false;
        }

        float speed = WalkVelocityScaler(_x_axis_value) * pushingSpeed;
        rig.AddForce(new Vector2(50 * (speed - rig.velocity.x), 0));
    }

    void PickState(bool transition)
    {
        PlayerManager.state = PlayerManager.StateCode.Action_pick;
        if (transition) { isFirstFrame = true; return; }
        /*------------Start of State Transitions------------*/
        if (isFirstFrame)
        {
            isFirstFrame = false;
            portrait.CrossFade(Animation_Pick, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
            portrait.CrossFadeQueued(Animation_Idle_Weapon);
        }
        else if(!portrait.IsPlaying(Animation_Pick))
        {
            PlayerManager.isTaking = true;
            Pickable.isTaking = false;
            PlayerManager.state = PlayerManager.StateCode.Idle;
            return;
        }
        /*------------End of State Transitions------------*/
    }

    public void Climb()
    {
        ClimbState(true);
    }
    Vector2 ClimbPosition()
    {
        return new Vector2((orient ? -ClimbOffset : ClimbOffset) + Climbable.climbed.transform.position.x, Climbable.climbed.transform.position.y);
    }
    Quaternion ClimbRotation()
    {
        Vector3 Euler = Climbable.climbed.transform.rotation.eulerAngles;
        return Quaternion.Euler(Euler.x, Euler.y, Euler.z + 90);
    }
    void ClimbState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Climb; isFirstFrame = true; return; }

        /*------------Start of State Transitions------------*/

        if (input.GetKeyDown(InputAction.Interact) && !isFirstFrame)
        {
            rig.simulated = true;
            rig.velocity = Vector2.zero;
            Climbable.climbed = null;
            isFirstFrame = true;
            FallState();
            return;
        }

        /*------------End of State Transitions------------*/

        if (isFirstFrame)
        {
            isFirstFrame = false;
            if(PlayerManager.onGround)
            {
                portrait.CrossFade(Animation_Climb_Start);
                portrait.CrossFadeQueued(Animation_Climb_Idle);
            }
            else
            {
                portrait.CrossFade(Animation_Climb_Idle);
            }
        }
        if(input.GetKey(InputAction.Right))
        {
            orient = true;
            gameObject.transform.localScale = new Vector3(-Scale.x, Scale.y, Scale.z);
            Climbable.climbed.rig.velocity += new Vector2(chainnn,0);
        }
        else if(input.GetKey(InputAction.Left))
        {
            orient = false;
            gameObject.transform.localScale = new Vector3(Scale.x, Scale.y, Scale.z);
            Climbable.climbed.rig.velocity -= new Vector2(chainnn, 0);
        }

        
        rig.simulated = false;
        transform.position = ClimbPosition();
        transform.rotation = ClimbRotation();
    }



    IEnumerator _CollisionDetectionHelper()
    {
        while(true)
        {
            yield return Wait100ms;
            Vector2 pos = (Vector2)PlayerManager.instance.player.transform.position + new Vector2(0.069f, -1.7f);
            Collider2D collider = Physics2D.OverlapBox(pos, new Vector2(0.8f, 0.01f), 0, groundLayer);
            if (collider == null)
            {
                PlayerManager.onGround = false;
            }
            else
            {
                PlayerManager.onGround = true;
            }
        }
    }

    public void SetJump(bool jumpAble)
    {
        _isJumpAble = jumpAble;
    }
 
    void UpdateIsSprinting()
    {
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
    }

    bool UpdateOrientation()
    {
        if (_isMoveable)
        {
            if (rig.velocity.x > 0.1f && (!orient))
            {
                gameObject.transform.localScale = new Vector3(-Scale.x, Scale.y, Scale.z);
                orient = true;
                return true;
            }
            else if (rig.velocity.x < -0.1f && orient)
            {
                gameObject.transform.localScale = new Vector3(Scale.x, Scale.y, Scale.z);
                orient = false;
                return true;
            }
        }
        return false;
    }

    //private void OnDrawGizmos()
    //{
    //    Vector2 pos = new Vector2(-0.861f, 4.947f) + new Vector2(0.069f, -1.75f);//1.75  2.0
    //    Gizmos.DrawCube(pos, new Vector2(0.8f, 0.01f));
    //}
}