using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnyPortrait;
using UnityEditor;

public class PlayerMovement : MonoBehaviour
{
    public enum EventType
    {
        Run, Jump, Land, Die, Stumble
    };
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2;
    [SerializeField] private float runSpeed = 5;
    [SerializeField] private float pushingSpeed = 1;
    [SerializeField] private float SprintingSpeed = 5.5f;
    [SerializeField] private float BrakingSpeed = 7.5f;
    [SerializeField] private float SpeedInAir = 2;
    [SerializeField] private float TimeToRest = 4;
    [SerializeField] private float RestSpeed = 2;
    [SerializeField] private float Run_StaminaCost = 2;
    [SerializeField] private float PortSpeed = 1;
    [SerializeField] private float WalkTimeToRun = 1;
    private float WalkingTime = 0;
    [SerializeField] private GameObject RunHint;
    [SerializeField] private TMPro.TextMeshPro RunHintText;
   

    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float lowJumpFactor = 0.5f;
    [SerializeField] private float JumpForwardFactor = 1;
    [SerializeField] private float Jump_StaminaCost = 10;

    private bool _isJumpAble = true;
    public bool _isMoveable { get; private set; } = true;
    private InputManager input;
    [Header("PickWeapon")]
    [SerializeField] private float TimeToReleaseWeapon = 0.5f;
    [Header("Climb")]
    [SerializeField] private Vector2 ClimbOffset;
    [SerializeField] private float ClimbSpeed = 1;
    [SerializeField] private float SwingForce = 5;
    [SerializeField] private Vector2 FootPosition;
    [SerializeField] private Vector2 ClimbJumpForce;
    [SerializeField] private float ClimbFootForce = 50;

    private apControlParam Swing_parameter;
    [Header("Fall Damage")]
    [SerializeField] private float SpeedToDie = -4;
    [Header("Crawl")]
    [SerializeField] private float CrawlSpeed = 3;
    [Header("Swim")]
    [SerializeField] private float SpeedInWater = 1;
    [SerializeField] private float JumpForceInWater = 5;
    [SerializeField] private float SwimDownSpeed = -5;
    [SerializeField] private float TerminalSpeedInWater = -1;
    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask staticColliderLayer;
    [Header("Stumble")]
    [SerializeField] private float Stumble_StandUp_time;
    [SerializeField] private Vector2 Stumble_Force;
    private apPortrait portrait;
    public static PlayerMovement instance;
    public bool orient { get; private set; } = false;                        //True means the player is facing right.False means the player is facing left.

    public bool isSprinting { get; private set; } = false;
    public Rigidbody2D rig { get; private set; }
    private CapsuleCollider2D m_collider;
    private float capsuleRadius;
    private bool isFullSpeed = false;
    private bool isFirstFrame = true;
    Vector3 Scale;                                      //The scale of the main character.
    WaitForSeconds Wait100ms = new WaitForSeconds(0.1f);
    WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    List<Action<bool>> States;
    #region EventAction
    [HideInInspector] public event Action OnRunning;
    [HideInInspector] public event Action OnJump;
    [HideInInspector] public event Action OnLanding;
    [HideInInspector] public event Action OnDie;
    [HideInInspector] public event Action OnStumble;
    #endregion

    #region Animation_String
    const string Animation_Idle = "Idle";
    const string Animation_Walk = "Walk";
    const string Animation_Run = "Run";
    const string Animation_Jump = "Jump";
    const string Animation_Fall = "Fall";
    const string Animation_Land = "Land";
    const string Animation_Brake = "Stop";
    const string Animation_Push_Start = "StartPush";
    const string Animation_Push = "Push";
    const string Animation_Push_End = "EndPush";
    const string Animation_Pull_Start = "StartPull";
    const string Animation_Pull = "Pull";
    const string Animation_Pull_End = "EndPull";
    const string Animation_Pick = "Pick";
    const string Animation_Idle_Weapon = "IdleWithWeapon";
    const string Animation_Pick_PutDown = "Abandon";
    const string Animation_Climb_Start = "StartClimb";
    const string Animation_Climb_Idle = "StayOnRope";
    const string Animation_Climb_Move = "Climb";
    const string Animation_Climb_End = "EndClimb";
    const string Animation_Die = "Die";
    const string Animation_Reborn = "Reborn";
    const string Animation_Crawl_Start = "DropDown";
    const string Animation_Crawl = "Crawl";
    const string Animation_Crawl_StandUp = "StandUp";
    const string Animation_Stumble = "FallDown";
    const string Animation_Attack = "Attack";
    const string Animation_Swim = "Swim";
    const string Animation_Float = "Float";
    const string Animation_Swing = "ShakeRope";
    const string Animation_Port_Start = "Take";
    const string Animation_Port = "Handle";
    const string Animation_Port_Walk = "HandleWalk";
    const string Animation_Port_End = "Put";
    const string Shader_Dissolve = "_DissolveValue";
    float DissolveValue = 0;
    #endregion
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            rig = GetComponent<Rigidbody2D>();
            m_collider = GetComponent<CapsuleCollider2D>();
            portrait = GetComponent<apPortrait>();
            Scale = transform.localScale;
            input = PlayerManager.instance.input;
            PlayerManager.state = PlayerManager.StateCode.Idle;
            capsuleRadius = GetComponent<CapsuleCollider2D>().size.y / 4.0f;
            StartCoroutine(nameof(_CollisionDetectionHelper));
            DontDestroyOnLoad(this.gameObject);
            PrepareStateMachine();
            Swing_parameter = portrait.GetControlParam("Swing");
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
    void PrepareStateMachine()
    {
        States = new List<Action<bool>>();
        States.AddRange(System.Linq.Enumerable.Repeat(default(Action<bool>), (int)PlayerManager.StateCode.None + 1));
        States[(int)PlayerManager.StateCode.Idle] = IdleState;
        States[(int)PlayerManager.StateCode.Walk] = WalkState;
        States[(int)PlayerManager.StateCode.Run] = RunState;
        States[(int)PlayerManager.StateCode.Brake] = BrakeState;
        States[(int)PlayerManager.StateCode.Jump] = JumpState;
        States[(int)PlayerManager.StateCode.Fall] = FallState;
        States[(int)PlayerManager.StateCode.Action_move_object] = MoveObjectState;
        States[(int)PlayerManager.StateCode.Action_pick] = PickState;
        States[(int)PlayerManager.StateCode.Climb] = ClimbState;
        States[(int)PlayerManager.StateCode.Die] = DieState;
        States[(int)PlayerManager.StateCode.Reborn] = RebornState;
        States[(int)PlayerManager.StateCode.Crawl] = CrawlState;
        States[(int)PlayerManager.StateCode.Stumble] = StumbleState;
        States[(int)PlayerManager.StateCode.Attack] = AttackState;
        States[(int)PlayerManager.StateCode.Swim] = SwimState;
        States[(int)PlayerManager.StateCode.Float] = FloatState;
        States[(int)PlayerManager.StateCode.Swing] = SwingState;
        States[(int)PlayerManager.StateCode.Action_port_idle] = PortIdleState;
        States[(int)PlayerManager.StateCode.Action_port_walk] = PortWalkState;
        States[(int)PlayerManager.StateCode.None] = (bool a) => { };

    }
    

    void Movement()//Calculate her speed and detect braking state
    {
        UpdateIsSprinting();
        States[(int)PlayerManager.state](false);
        if (Mathf.Abs(rig.velocity.x) < walkSpeed)
        {
            isFullSpeed = false;
        }
    }


    bool BrakeEnd = true;
    void BrakeState(bool transition)// ( completed )
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Brake; setIsFirstFrame(true); return; }


        /*------------Start of State Transitions------------*/
        //idle
        if ((!isFirstFrame) && BrakeEnd)
        {
            _isMoveable = true;
            IdleState(true);
            return;
        }
        //fall
        if (!PlayerManager.onGround && rig.velocity.y < -1)
        {
            _isMoveable = true;
            FallState(true);
            return;
        }
        /*------------End of State Transitions------------*/


        _isMoveable = false;
        isSprinting = false;
        if (isFirstFrame)
        {
            isFirstFrame = false;
            BrakeEnd = false;
            portrait.CrossFade(Animation_Brake);
            PlayerManager.instance.isFreeToDoAction = false;
            PlayerManager.instance.CanWalkOnStairs = false;
        }
        if (SlowDown(16))
        {
            _isMoveable = true;
            BrakeEnd = true;
        }
    }

    public bool SlowDown(float force)
    {
        if (Mathf.Abs(rig.velocity.x) > 0.1f)
        {
            rig.AddForce(new Vector2(-rig.velocity.x * force, 0));
            return false;
        }
        else
        {
            rig.velocity = new Vector2(0, rig.velocity.y);
            return true;
        }
    }


    float resting_time = 0;
    void IdleState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Idle; setIsFirstFrame(true); return; }

        /*------------Start of State Transitions------------*/
        //walk
        if (input.GetHorizonInput() != 0)
        {
            WalkState(true);
            return;
        }
        //crawl
        if (input.GetKeyDown(InputAction.Down))
        {
            CrawlState(true);
            return;
        }
        //ride
        //float
        if (PlayerManager.instance.isInWater)
        {
            FloatState(true);
            return;
        }
        //attack
        if (PlayerManager.isTaking && Pickable.held.isWeapon && input.GetKey(InputAction.Attack))
        {
            AttackState(true);
            return;
        }
        //take
        {
            //Push
            if (Moveable.ready2move)
            {
                MoveObjectState(true);
                return;
            }
            //Pick
            if (Pickable.isTaking)
            {
                PickState(true);
                return;
            }
            //Put
            if (PlayerManager.isTaking && input.GetKeyDown(InputAction.Interact))
            {
                PlayerManager.isTaking = false;
                StartCoroutine(nameof(PutDownWeapon));
                return;
            }
            //port
            if (Portable.ready2port)
            {
                PortIdleState(true);
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
            FallState(true);
            return;
        }
        /*------------End of State Transitions------------*/

        resting_time += Time.deltaTime;
        if (resting_time > TimeToRest && PlayerManager.instance.Stamina < 100)
        {
            PlayerManager.instance.Stamina += Time.deltaTime * RestSpeed;
            if (PlayerManager.instance.Stamina > 100) { PlayerManager.instance.Stamina = 100; }
        }
        if (isFirstFrame)
        {
            portrait.CrossFade(PlayerManager.isTaking && Pickable.held.isWeapon ? Animation_Idle_Weapon : Animation_Idle);
            isFirstFrame = false;
            if (Pickable.held == null)
            {
                PlayerManager.instance.isFreeToDoAction = true;
            }
            PlayerManager.instance.CanWalkOnStairs = true;
        }
        isSprinting = false;
    }

    IEnumerator PutDownWeapon()
    {
        PlayerManager.state = PlayerManager.StateCode.None;
        float time = 0;
        portrait.CrossFade(Animation_Pick_PutDown, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
        Pickable.held.PutDown();
        yield return waitForEndOfFrame;
        while (portrait.IsPlaying(Animation_Pick_PutDown))
        {
            SlowDown(32);
            if (Pickable.held != null)
            {
                time += Time.deltaTime;
                if (time > TimeToReleaseWeapon)
                {
                    Pickable.held.Throw(Vector2.zero);
                }
            }
            yield return waitForEndOfFrame;
        }
        IdleState(true);
    }

    float idle_time = 0;
    void WalkState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Walk; setIsFirstFrame(true); return; }


        /*------------Start of State Transitions------------*/
        //idle
        if (Mathf.Abs(rig.velocity.x) < 0.1f && (input.GetHorizonInput() == 0))
        {
            if (idle_time > 0.2f)
            {
                rig.velocity = new Vector2(0, rig.velocity.y);
                IdleState(true);
                idle_time = 0;
                return;
            }
            else
            {
                idle_time += Time.deltaTime;
            }
        }
        if (PlayerManager.isTaking && Pickable.held.isWeapon && input.GetKey(InputAction.Attack))
        {
            AttackState(true);
            return;
        }
        //run
        if (isSprinting)
        {
            RunState(true);
            return;
        }
        //crawl
        if (input.GetKeyDown(InputAction.Down))
        {
            CrawlState(true);
            return;
        }
        //float
        if (PlayerManager.instance.isInWater)
        {
            FloatState(true);
            return;
        }
        //take
        {
            //Push
            if (Moveable.ready2move)
            {
                MoveObjectState(true);
                return;
            }
            //Pick
            if (Pickable.isTaking)
            {
                PickState(true);
                return;
            }
            //Put
            if (PlayerManager.isTaking && input.GetKeyDown(InputAction.Interact))
            {
                PlayerManager.isTaking = false;
                StartCoroutine(nameof(PutDownWeapon));
                return;
            }
            //port
            if (Portable.ready2port)
            {
                PortIdleState(true);
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
            FallState(true);
            return;
        }
        /*------------End of State Transitions------------*/


        UpdateOrientation();
        float speed = input.GetKey(InputAction.Right) ? walkSpeed : (input.GetKey(InputAction.Left) ? -walkSpeed : 0);//WalkVelocityScaler(_x_axis_value) * walkSpeed;
        rig.AddForce(new Vector2(16 * (speed - rig.velocity.x), 0));
        resting_time += Time.deltaTime;
        if (resting_time > TimeToRest && PlayerManager.instance.Stamina < 100)
        {
            PlayerManager.instance.Stamina += Time.deltaTime * RestSpeed;
            if (PlayerManager.instance.Stamina > 100) { PlayerManager.instance.Stamina = 100; }
        }
        if (isFirstFrame)
        {
            WalkingTime = 0;
            portrait.CrossFade(Animation_Walk);
            isFirstFrame = false;
            if (Pickable.held == null)
            {
                PlayerManager.instance.isFreeToDoAction = true;
            }
            PlayerManager.instance.CanWalkOnStairs = true;
        }
        isSprinting = false;
    }

    void RunState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Run; setIsFirstFrame(true); return; }


        /*------------Start of State Transitions------------*/
        //Brake
        if (((Mathf.Abs(rig.velocity.x) < BrakingSpeed || !isSprinting) && isFullSpeed) || (Mathf.Abs(rig.velocity.x) < walkSpeed && input.GetHorizonInput() == 0))
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
            FallState(true);
            return;
        }
        //Stumble
        if (PlayerManager.instance.Stamina <= 0)
        {
            PlayerManager.instance.Stamina = 0;
            StumbleState(true);
            return;
        }
        //float
        if (PlayerManager.instance.isInWater)
        {
            FloatState(true);
            return;
        }
        /*------------End of State Transitions------------*/

        PlayerManager.instance.Stamina -= Time.deltaTime * Run_StaminaCost;
        resting_time = 0;
        float speed = input.GetHorizonInput() * runSpeed;
        rig.AddForce(new Vector2(16 * (speed - rig.velocity.x), 0));
        if (Mathf.Abs(rig.velocity.x) > BrakingSpeed)
        {
            isFullSpeed = true;
        }
        if (isFirstFrame)
        {
            portrait.CrossFade(Animation_Run);
            isFirstFrame = false;
            PlayerManager.instance.isFreeToDoAction = false;
            PlayerManager.instance.CanWalkOnStairs = true;
        }
        if (Mathf.Abs(rig.velocity.x) < float.Epsilon)
        {
            isSprinting = false;
        }
        OnRunning?.Invoke();
        UpdateOrientation();
    }

    void JumpState(bool transition)//Jumping state detection and animation
    {
        if (transition) { if (PlayerManager.instance.Stamina < Jump_StaminaCost) { return; }; PlayerManager.state = PlayerManager.StateCode.Jump; setIsFirstFrame(true); return; }
        //Additional speed boost
        {
            //high jump
            if (rig.velocity.y > 0 && input.GetKeyUp(InputAction.Jump))
            {
                rig.velocity = new Vector2(rig.velocity.x, rig.velocity.y * lowJumpFactor);
            }

            //Horizontal movement

            if (input.GetKey(InputAction.Right) && rig.velocity.x < SpeedInAir && rig.velocity.x > -0.001f)
            {
                rig.AddForce(new Vector2(8 * (SpeedInAir - rig.velocity.x), 0));
            }
            else if (input.GetKey(InputAction.Left) && rig.velocity.x > -SpeedInAir && rig.velocity.x < 0.001f)
            {
                rig.AddForce(new Vector2(8 * (rig.velocity.x - SpeedInAir), 0));
            }

        }

        if (isFirstFrame && _isMoveable)
        {
            if (PlayerManager.onGround && _isJumpAble)
            {
                PlayerManager.instance.Stamina -= Jump_StaminaCost;
                resting_time = 0;
                OnJump?.Invoke();
                rig.AddForce(new Vector2(rig.velocity.x * JumpForwardFactor, jumpForce), ForceMode2D.Impulse);
            }
        }

        /*------------Start of State Transitions------------*/
        //idle
        if (Mathf.Abs(rig.velocity.x) < 0.01f && Mathf.Abs(rig.velocity.y) < 0.01f && PlayerManager.onGround)
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
        if (PlayerManager.instance.isInWater)
        {
            FloatState(true);
            return;
        }
        //fall
        if (rig.velocity.y < 0 && !PlayerManager.onGround)
        {
            FallState(true);
            return;
        }
        /*------------End of State Transitions------------*/

        UpdateOrientation();
        if (isFirstFrame)
        {
            portrait.CrossFade(Animation_Jump);
            portrait.CrossFadeQueued(Animation_Fall);
            isFirstFrame = false;
            PlayerManager.instance.CanWalkOnStairs = true;
        }
        if (Mathf.Abs(rig.velocity.x) < walkSpeed)
        {
            isSprinting = false;
        }
    }


    float fallingSpeed = 0;
    void FallState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Fall; setIsFirstFrame(true); return; }

        /*------------Start of State Transitions------------*/
        if (PlayerManager.onGround)
        {
            float speed = Mathf.Abs(rig.velocity.x);
            //die
            if (fallingSpeed < SpeedToDie)
            {
                Killed();
                return;
            }
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
        else if (PlayerManager.instance.isInWater)
        {
            FloatState(true);
            return;
        }
        /*------------End of State Transitions------------*/

        //Horizontal movement

        if (input.GetKey(InputAction.Right) && rig.velocity.x < SpeedInAir && rig.velocity.x > -0.001f)
        {
            rig.AddForce(new Vector2(8 * (SpeedInAir - rig.velocity.x), 0));
        }
        else if (input.GetKey(InputAction.Left) && rig.velocity.x > -SpeedInAir && rig.velocity.x < 0.001f)
        {
            rig.AddForce(new Vector2(8 * (rig.velocity.x - SpeedInAir), 0));
        }
        UpdateOrientation();

        if (isFirstFrame)
        {
            portrait.CrossFade(Animation_Fall);
            isFirstFrame = false;
            PlayerManager.instance.CanWalkOnStairs = true;
        }
        if (Mathf.Abs(rig.velocity.x) < walkSpeed)
        {
            isSprinting = false;
        }
        if (rig.velocity.y < -0.1)
        {
            fallingSpeed = rig.velocity.y;
        }
    }

    void LandState()
    {
        portrait.CrossFade(Animation_Land);
        if (!portrait.IsPlaying(Animation_Land))
        {
            IdleState(true);
            return;
        }
    }


    float MoveObjectLagTime = 0;
    void MoveObjectState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Action_move_object; setIsFirstFrame(true); return; }
        /*------------Start of State Transitions------------*/
        if((Mathf.Abs(rig.velocity.x) < 0.1f && input.GetHorizonInput() == 0) || !Input.GetKey(KeyCode.Mouse0))
        {
            MoveObjectLagTime += Time.deltaTime;
        }
        else
        {
            MoveObjectLagTime = 0;
        }


        if (MoveObjectLagTime >= 0.2f || Moveable.moved == null)
        {
            Moveable.ready2move = false;
            if(Moveable.moved != null)
            {
                Moveable.moved.joint.enabled = false;
            }
            PlayerManager.instance.isFreeToDoAction = true;
            IdleState(true);
            return;
        }
        if (rig.velocity.y < 0 && !PlayerManager.onGround)
        {
            LeaveMoveObjectState();
            FallState(true);
        }

        if (((Moveable.moved.transform.position - PlayerManager.instance.player.transform.position).x > 0) == input.GetKey(InputAction.Right))
        {
            //push
            if (portrait.IsPlaying(Animation_Pull))
            {
                MoveObjectState(true);
                return;
            }
        }
        else
        {
            //pull
            if (portrait.IsPlaying(Animation_Push))
            {
                MoveObjectState(true);
                return;
            }
        }
        
        /*------------End of State Transitions------------*/
        if (isFirstFrame)
        {
            if (input.GetHorizonInput() != 0)
            {
                if (((Moveable.moved.transform.position - PlayerManager.instance.player.transform.position).x > 0) == input.GetKey(InputAction.Right))
                {
                    //push
                    portrait.CrossFade(Animation_Push_Start);
                    portrait.CrossFadeQueued(Animation_Push);
                    Moveable.moved.Start2Move(true);
                }
                else
                {
                    //pull
                    portrait.CrossFade(Animation_Pull_Start);
                    portrait.CrossFadeQueued(Animation_Pull);
                    if ((Moveable.moved.transform.position.x > PlayerManager.instance.player.transform.position.x))
                    {
                        if (orient == false)
                        {
                            orient = true;
                            gameObject.transform.localScale = new Vector3(-Scale.x, Scale.y, Scale.z);
                        }
                    }
                    else
                    {
                        if (orient == true)
                        {
                            orient = false;
                            gameObject.transform.localScale = new Vector3(Scale.x, Scale.y, Scale.z);
                        }
                    }
                    Moveable.moved.Start2Move(false);
                }
                Moveable.moved.joint.connectedBody = rig;
                Moveable.moved.joint.enabled = true;
                isFirstFrame = false;
                PlayerManager.instance.CanWalkOnStairs = false;
            }
        }

        float speed = input.GetHorizonInput() * pushingSpeed;
        Moveable.moved.rig.velocity = new Vector2(speed, 0);
    }

    void LeaveMoveObjectState()
    {
        if (Moveable.moved != null)
        {
            Moveable.moved.joint.enabled = false;
            Moveable.moved = null;
        }
        Moveable.ready2move = false;
        PlayerManager.instance.isFreeToDoAction = true;
    }

    void PickState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Action_pick; setIsFirstFrame(true); return; }
        /*------------Start of State Transitions------------*/
        if (isFirstFrame)
        {
            isFirstFrame = false;
            portrait.CrossFade(Animation_Pick, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
            portrait.CrossFadeQueued(Animation_Idle_Weapon);
        }
        else if (!portrait.IsPlaying(Animation_Pick))
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
        return Vector2.Lerp(Climbable.climbed.GetBottom(), Climbable.climbed.GetTop(), Climbable_pos / Climbable.climbed.length);
    }

    //float ClimbRotation()
    //{
    //    float A = Climbable.climbed.transform.rotation.eulerAngles.z;
    //    float rotation = A;
    //    if(Climbable.climbed.Up != null)
    //    {
    //        float B = Climbable.climbed.Up.transform.rotation.eulerAngles.z;
    //        rotation = Mathf.Lerp(A, B, Climbable_pos / Climbable.climbed.length);
    //    }
    //    return rotation + 90;
    //}


    bool Climbable_StartMoving = true;
    RelativeJoint2D ClimbJoint;
    void ClimbState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Climb; setIsFirstFrame(true); return; }

        /*------------Start of State Transitions------------*/
        if (input.GetKeyDown(InputAction.Jump))
        {
            LeaveClimbState();
            rig.AddForce(new Vector2(orient ? ClimbJumpForce.x : -ClimbJumpForce.x, ClimbJumpForce.y));
            FallState(true);
            return;
        }

        /*------------End of State Transitions------------*/
        if (EndingClimb) return;
        if (isFirstFrame)
        {
            isFirstFrame = false;
            isSprinting = false;
            isFullSpeed = false;
            Climbable_StartMoving = true;
            PlayerManager.instance.CanWalkOnStairs = false;
            if (PlayerManager.onGround)
            {
                portrait.CrossFade(Animation_Climb_Start);
                portrait.CrossFadeQueued(Animation_Climb_Idle);
            }
            else
            {
                portrait.CrossFade(Animation_Climb_Idle);
            }
            StartClimbEnd = false;
            StartCoroutine(nameof(StartToClimb));
        }
        if (input.GetKey(InputAction.Up))
        {
            float displacement = ClimbSpeed * Time.deltaTime;
            Climbable_pos += displacement;
            if (Climbable_pos > Climbable.climbed.length)
            {
                if (Climbable.climbed.Up != null)
                {
                    Climbable.climbed = Climbable.climbed.Up;
                    Climbable_pos = 0;
                    if (ClimbJoint != null)
                    {
                        ClimbJoint.connectedBody = Climbable.climbed.rig;
                        ClimbJoint.enabled = true;
                    }
                }
                else
                {
                    Climbable_pos = Climbable.climbed.length;
                    goto Out;
                }
            }
            if (Climbable.climbed_foot != null)
            {
                Climbable_pos_foot -= (displacement / Climbable.climbed_foot.transform.lossyScale.x * 0.5f);
                if (Climbable_pos_foot < -Climbable.climbed_foot.local_length_2)
                {
                    Climbable_pos_foot = Climbable.climbed_foot.local_length_2;
                    Climbable.climbed_foot.FootJoint.enabled = false;
                    Climbable.climbed_foot = Climbable.climbed_foot.Up;
                }
                if (Climbable.climbed_foot != null)
                {
                    if (Climbable.climbed_foot.FootJoint == null)
                    {
                        Climbable.climbed_foot.FootJoint = Climbable.climbed_foot.gameObject.AddComponent<TargetJoint2D>();
                        Climbable.climbed_foot.FootJoint.autoConfigureTarget = false;
                        Climbable.climbed_foot.FootJoint.maxForce = ClimbFootForce;
                        Climbable.climbed_foot.FootJoint.dampingRatio = 1;
                        Climbable.climbed_foot.FootJoint.frequency = 1000000;
                    }
                    Climbable.climbed_foot.FootJoint.enabled = true;
                    Climbable.climbed_foot.FootJoint.target = transform.TransformPoint(FootPosition);
                    Climbable.climbed_foot.FootJoint.anchor = new Vector2(Climbable_pos_foot, 0);
                }
            }
            if (Climbable_StartMoving)
            {
                Climbable_StartMoving = false;
                portrait.CrossFade(Animation_Climb_Move);
                portrait.SetAnimationSpeed(Animation_Climb_Move, 1);
            }
        }
        else if (input.GetKey(InputAction.Down))
        {
            if(PlayerManager.onGround)
            {
                portrait.CrossFade(Animation_Climb_End, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
                StartCoroutine(nameof(EndClimb));
                return;
            }
            float displacement = ClimbSpeed * Time.deltaTime;
            Climbable_pos -= displacement;
            if (Climbable_pos < 0)
            {
                if (Climbable.climbed.Down != null)
                {
                    Climbable.climbed = Climbable.climbed.Down;
                    Climbable_pos = Climbable.climbed.length;
                    if (ClimbJoint != null)
                    {
                        ClimbJoint.connectedBody = Climbable.climbed.rig;
                        ClimbJoint.enabled = true;
                    }
                }
                else
                {
                    portrait.CrossFade(Animation_Climb_End, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
                    StartCoroutine(nameof(EndClimb));
                    goto Out;
                }
            }
            if (Climbable.climbed_foot != null)
            {
                Climbable_pos_foot += (displacement / Climbable.climbed_foot.transform.lossyScale.x * 0.5f);
                if (Climbable_pos_foot > Climbable.climbed_foot.local_length_2)
                {
                    Climbable_pos_foot = -Climbable.climbed_foot.local_length_2;
                    Climbable.climbed_foot.FootJoint.enabled = false;
                    Climbable.climbed_foot = Climbable.climbed_foot.Down;
                }
                if (Climbable.climbed_foot != null)
                {
                    if (Climbable.climbed_foot.FootJoint == null)
                    {
                        Climbable.climbed_foot.FootJoint = Climbable.climbed_foot.gameObject.AddComponent<TargetJoint2D>();
                        Climbable.climbed_foot.FootJoint.autoConfigureTarget = false;
                        Climbable.climbed_foot.FootJoint.maxForce = ClimbFootForce;
                        Climbable.climbed_foot.FootJoint.dampingRatio = 1;
                        Climbable.climbed_foot.FootJoint.frequency = 1000000;
                    }
                    Climbable.climbed_foot.FootJoint.enabled = true;
                    Climbable.climbed_foot.FootJoint.target = transform.TransformPoint(FootPosition);
                    Climbable.climbed_foot.FootJoint.anchor = new Vector2(Climbable_pos_foot, 0);
                }
            }
            if (Climbable_StartMoving)
            {
                Climbable_StartMoving = false;
                portrait.CrossFade(Animation_Climb_Move);
                portrait.SetAnimationSpeed(Animation_Climb_Move, -1);
            }
        }
        else if (input.GetHorizonInput() != 0)
        {
            SwingState(true);
            return;
        }
        else if (Climbable_StartMoving == false)
        {
            Climbable_StartMoving = true;
            portrait.CrossFade(Animation_Climb_Idle);
        }
    Out:
        if (StartClimbEnd)
        {
            Vector2 hand = transform.TransformPoint(ClimbOffset);
            Vector3 dif = ClimbPosition() - hand;
            transform.position += dif;
            //float dif_rot = ClimbRotation() - transform.rotation.eulerAngles.z;
            //transform.RotateAround(hand, Vector3.forward, dif_rot);
            if (Climbable.climbed_foot != null)
            {
                if (Climbable.climbed_foot.FootJoint != null)
                {
                    Climbable.climbed_foot.FootJoint.target = transform.TransformPoint(FootPosition);
                }
                else
                {
                    Climbable.climbed_foot.FootJoint = Climbable.climbed_foot.gameObject.AddComponent<TargetJoint2D>();
                    Climbable.climbed_foot.FootJoint.autoConfigureTarget = false;
                    Climbable.climbed_foot.FootJoint.maxForce = ClimbFootForce;
                    Climbable.climbed_foot.FootJoint.dampingRatio = 1;
                    Climbable.climbed_foot.FootJoint.frequency = 1000000;
                }
            }
            else
            {
                Climbable.climbed_foot = Climbable.climbed.FindClosestFoot(transform.TransformPoint(FootPosition));
            }
        }
    }

    void LeaveClimbState()
    {
        if (ClimbJoint != null)
        {
            ClimbJoint.enabled = false;
        }
        if (Climbable.climbed != null)
        {
            rig.simulated = true;
            rig.velocity = Climbable.climbed.rig.velocity + new Vector2(orient ? 2 : -2, 2);
            transform.rotation = Quaternion.identity;
            Climbable.climbed = null;
            PlayerManager.instance.isFreeToDoAction = true;
        }
        if (Climbable.climbed_foot != null && Climbable.climbed_foot.FootJoint != null)
        {
            Climbable.climbed_foot.FootJoint.enabled = false;
        }
    }

    float Swing_Current_Frame = 0;
    void SwingState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Swing; setIsFirstFrame(true); return; }

        if (EndSwingend) return;
        if (input.GetKeyDown(InputAction.Jump))
        {
            portrait.SetControlParamFloat(Swing_parameter, -2);
            LeaveClimbState();
            rig.AddForce(new Vector2(orient ? ClimbJumpForce.x : -ClimbJumpForce.x, ClimbJumpForce.y));
            FallState(true);
            return;
        }
        if (input.GetKey(InputAction.Right))
        {
            if (orient == false)
            {
                Vector3 test = m_collider.transform.TransformPoint(new Vector3(2 * ClimbOffset.x, 0, 0));
                Collider2D col = Physics2D.OverlapCapsule(test, m_collider.size * m_collider.transform.lossyScale, CapsuleDirection2D.Vertical, m_collider.transform.rotation.eulerAngles.z, groundLayer);
                if (col == null)
                {
                    orient = true;
                    Climbable.climbed.rig.velocity += new Vector2(10, 0);
                    gameObject.transform.localScale = new Vector3(-Scale.x, Scale.y, Scale.z);
                }
            }
            else
            {
                Climbable.climbed.rig.velocity += new Vector2(SwingForce, 0);
            }
        }
        else if (input.GetKey(InputAction.Left))
        {
            if (orient == true)
            {
                Vector3 test = m_collider.transform.TransformPoint(new Vector3(2 * ClimbOffset.x, 0, 0));
                Collider2D col = Physics2D.OverlapCapsule(test, m_collider.size * m_collider.transform.lossyScale, CapsuleDirection2D.Vertical, m_collider.transform.rotation.eulerAngles.z, groundLayer);
                if (col == null)
                {
                    orient = false;
                    Climbable.climbed.rig.velocity -= new Vector2(10, 0);
                    gameObject.transform.localScale = new Vector3(Scale.x, Scale.y, Scale.z);
                }
            }
            else
            {
                Climbable.climbed.rig.velocity -= new Vector2(SwingForce, 0);
            }
        }
        else
        {
            StartCoroutine(nameof(EndSwing));
            return;
        }
        if (StartClimbEnd)
        {
            Vector2 hand = transform.TransformPoint(ClimbOffset);
            Vector3 dif = ClimbPosition() - hand;
            transform.position += dif;
            //float dif_rot = ClimbRotation() - transform.rotation.eulerAngles.z;
            //transform.RotateAround(hand, Vector3.forward, dif_rot);
            if (Climbable.climbed_foot != null)
            {
                if (Climbable.climbed_foot.FootJoint != null)
                {
                    Climbable.climbed_foot.FootJoint.target = transform.TransformPoint(FootPosition);
                }
                else
                {
                    Climbable.climbed_foot.FootJoint = Climbable.climbed_foot.gameObject.AddComponent<TargetJoint2D>();
                    Climbable.climbed_foot.FootJoint.autoConfigureTarget = false;
                    Climbable.climbed_foot.FootJoint.maxForce = ClimbFootForce;
                    Climbable.climbed_foot.FootJoint.dampingRatio = 1;
                    Climbable.climbed_foot.FootJoint.frequency = 1000000;
                }
            }
            else
            {
                Climbable.climbed_foot = Climbable.climbed.FindClosestFoot(transform.TransformPoint(FootPosition));
            }
        }
        if (isFirstFrame)
        {
            isFirstFrame = false;
            portrait.StopAll();
            PlayerManager.instance.CanWalkOnStairs = false;
        }
        float speed = Mathf.Abs(rig.velocity.x);
        float frame = (SwingAnimationFactor * speed + 15) * (rig.velocity.y > 0 ? Mathf.Exp(-SwingAnimationFactor * speed) : -Mathf.Exp(-SwingAnimationFactor * speed)) + 15;
        Swing_Current_Frame += (frame - Swing_Current_Frame) * Time.deltaTime;
        portrait.SetControlParamFloat(Swing_parameter, Swing_Current_Frame);
    }


    const float EndSwingTime = 0.5f;

    bool EndSwingend = false;
    IEnumerator EndSwing()
    {
        EndSwingend = true;
        float time = 0;
        while (time < EndSwingTime)
        {
            Swing_Current_Frame += (-Swing_Current_Frame) * Time.deltaTime * 4;
            if (Swing_Current_Frame < 0)
            {
                portrait.SetControlParamFloat(Swing_parameter, 0);
                break;
            }
            portrait.SetControlParamFloat(Swing_parameter, Swing_Current_Frame);
            time += Time.deltaTime;
            if(Climbable.climbed_foot != null && Climbable.climbed_foot.FootJoint != null)
            {
                Climbable.climbed_foot.FootJoint.target = transform.TransformPoint(FootPosition);
            }
            yield return waitForEndOfFrame;
        }
        isFirstFrame = false;
        Swing_Current_Frame = 0;
        PlayerManager.state = PlayerManager.StateCode.Climb;
        portrait.SetControlParamFloat(Swing_parameter, -2);
        if (Climbable.climbed_foot != null && Climbable.climbed_foot.FootJoint != null)
        {
            Climbable.climbed_foot.FootJoint.target = transform.TransformPoint(FootPosition);
        }
        EndSwingend = false;
        portrait.Play(Animation_Climb_Idle);
    }

    const float SwingAnimationFactor = 1.4f;
    float Climbable_find_closest_pos(Climbable A, Vector2 position)
    {
        Vector2 vA = A.GetBottom();
        Vector2 vB = A.GetTop();
        Vector2 dir = (vB - vA).normalized;
        float t = Vector2.Dot(position - vA, dir);
        return Mathf.Clamp01(t);
    }


    float Climbable_pos = 0;
    float Climbable_pos_foot = 0;
    bool StartClimbEnd = false;

    IEnumerator StartToClimb()
    {
        rig.simulated = false;
        Climbable_pos = Climbable_find_closest_pos(Climbable.climbed, transform.position);
        Climbable.climbed_foot = Climbable.climbed.FindClosestFoot(transform.TransformPoint(FootPosition));
        if (Climbable.climbed_foot != null)
        {
            if (Climbable.climbed_foot.FootJoint == null)
            {
                Climbable.climbed_foot.FootJoint = Climbable.climbed_foot.gameObject.AddComponent<TargetJoint2D>();
            }
            Climbable_pos_foot = (1 - Climbable_find_closest_pos(Climbable.climbed_foot, transform.TransformPoint(FootPosition)) * 2) * Climbable.climbed_foot.local_length_2 + 0.4f;
            Climbable.climbed_foot.FootJoint.enabled = true;
            Climbable.climbed_foot.FootJoint.target = transform.TransformPoint(FootPosition);
            Climbable.climbed_foot.FootJoint.anchor = new Vector2(Climbable_pos_foot, 0);
            Climbable.climbed_foot.FootJoint.autoConfigureTarget = false;
            Climbable.climbed_foot.FootJoint.dampingRatio = 1;
            Climbable.climbed_foot.FootJoint.frequency = 1000000;
        }
        float acc_time = 0;
        while (acc_time < 0.5f)
        {
            acc_time += Time.deltaTime;
            Vector2 hand = transform.TransformPoint(ClimbOffset);
            Vector2 dif = ClimbPosition() - hand;
            transform.position += Vector3.Lerp(Vector2.zero, dif, acc_time * 2);
            if (Climbable.climbed_foot != null)
            {
                Climbable.climbed_foot.FootJoint.target = transform.TransformPoint(FootPosition);
                Climbable.climbed_foot.FootJoint.maxForce = ClimbFootForce * acc_time;
            }
            yield return waitForEndOfFrame;
        }

        if (ClimbJoint == null)
        {
            ClimbJoint = gameObject.AddComponent<RelativeJoint2D>();
        }
        ClimbJoint.enabled = true;
        ClimbJoint.connectedBody = Climbable.climbed.rig;
        StartClimbEnd = true;
        rig.simulated = true;
        if (Climbable.climbed_foot != null && Climbable.climbed_foot.FootJoint != null)
        {
            Climbable.climbed_foot.FootJoint.enabled = false;
        }
        Climbable.climbed_foot = Climbable.climbed.FindClosestFoot(transform.TransformPoint(FootPosition));
        if (Climbable.climbed_foot != null)
        {
            if (Climbable.climbed_foot.FootJoint == null)
            {
                Climbable.climbed_foot.FootJoint = Climbable.climbed_foot.gameObject.AddComponent<TargetJoint2D>();
            }
            Climbable_pos_foot = (1 - Climbable_find_closest_pos(Climbable.climbed_foot, transform.TransformPoint(FootPosition)) * 2) * Climbable.climbed_foot.local_length_2 + 0.4f;
            Climbable.climbed_foot.FootJoint.enabled = true;
            Climbable.climbed_foot.FootJoint.target = transform.TransformPoint(FootPosition);
            Climbable.climbed_foot.FootJoint.anchor = new Vector2(Climbable_pos_foot, 0);
            Climbable.climbed_foot.FootJoint.autoConfigureTarget = false;
            Climbable.climbed_foot.FootJoint.dampingRatio = 1;
            Climbable.climbed_foot.FootJoint.frequency = 1000000;
            Climbable.climbed_foot.FootJoint.maxForce = ClimbFootForce;
        }
    }


    bool EndingClimb = false;
    IEnumerator EndClimb()
    {
        EndingClimb = true;
        yield return new WaitUntil(() => { return !portrait.IsPlaying(Animation_Climb_End); });
        EndingClimb = false;
        rig.simulated = true;
        transform.rotation = Quaternion.identity;
        Climbable.climbed = null;
        if (Climbable.climbed_foot != null && Climbable.climbed_foot.FootJoint != null)
        {
            Climbable.climbed_foot.FootJoint.enabled = false;
        }
        PlayerManager.instance.isFreeToDoAction = true;
        ClimbJoint.enabled = false;
        FallState(true);
    }

    public void Killed()
    {
        LeaveAllState();
        rig.velocity = new Vector2(0, 0);
        DieState(true);
    }

    void DieState(bool transition)
    {
        if (transition) 
        {
            PlayerManager.state = PlayerManager.StateCode.Die;
            setIsFirstFrame(true);
            PlayerManager.instance.CanWalkOnStairs = false;
            PlayerManager.instance.isFreeToDoAction = false;
            return; }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RebornState(true);
            DissolveValue = 1;
            portrait.SetMeshCustomFloatAll(DissolveValue, Shader_Dissolve);
            return;
        }
        if (isFirstFrame)
        {
            isFirstFrame = false;
            OnDie?.Invoke();
            portrait.CrossFade(Animation_Die);
            DissolveValue = 0;
        }
        else if (DissolveValue != 1 && portrait.IsPlaying(Animation_Die))
        {
            DissolveValue += Time.deltaTime * 0.5f;
            if (DissolveValue > 1) { DissolveValue = 1; }
            portrait.SetMeshCustomFloatAll(DissolveValue, Shader_Dissolve);
        }
    }

    void RebornState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Reborn; setIsFirstFrame(true); return; }

        if (!isFirstFrame && !portrait.IsPlaying(Animation_Reborn))
        {
            IdleState(true);
            return;
        }

        if (isFirstFrame)
        {
            isFirstFrame = false;
            _isJumpAble = true;
            _isMoveable = true;
            isSprinting = false;
            isFullSpeed = false;
            PlayerManager.instance.Stamina = 100;
            portrait.CrossFade(Animation_Reborn, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
            PlayerManager.instance.isFreeToDoAction = false;
            DissolveValue = 1;
            StartCoroutine(nameof(RebornShader));
        }

    }
    IEnumerator RebornShader()
    {
        DissolveValue = 1;
        yield return new WaitForSeconds(3.3f);
        while(DissolveValue > 0)
        {
            portrait.SetMeshCustomFloatAll(DissolveValue, Shader_Dissolve);
            DissolveValue -= Time.deltaTime * 0.5f;
            yield return new WaitForEndOfFrame();
        }
        DissolveValue = 0;
        portrait.SetMeshCustomFloatAll(DissolveValue, Shader_Dissolve);
    }

    bool Crawling = false;

    bool EndCrawling = false;
    void CrawlState(bool transition)
    {
        if (transition) 
        {
            if (StaticCollider.isNotDropping)
            {
                PlayerManager.state = PlayerManager.StateCode.Crawl;
                setIsFirstFrame(true);
            }
            return; 
        }

        if (input.GetKeyDown(InputAction.Down))
        {
            EndCrawling = true;
            rig.velocity = Vector2.zero;
            LeaveCrawlState();
            return;
        }
        if (rig.velocity.y < 0 && !PlayerManager.onGround)
        {
            LeaveCrawlState();
            FallState(true);
            return;
        }
        if (EndCrawling)
        {
            if (!portrait.IsPlaying(Animation_Crawl_StandUp))
            {
                IdleState(true);
                EndCrawling = false;
            }
            return;
        }

        if (Crawling)
        {
            if (input.GetHorizonInput() != 0)
            {
                float speed = input.GetHorizonInput() * CrawlSpeed;
                rig.AddForce(new Vector2(16 * (speed - rig.velocity.x), 0));
                UpdateOrientation();
                portrait.SetAnimationSpeed(Animation_Crawl, 1);
            }
            else
            {
                portrait.SetAnimationSpeed(Animation_Crawl, 0);
            }
        }
        if (isFirstFrame)
        {
            isFirstFrame = false;
            Crawling = false;
            m_collider.direction = CapsuleDirection2D.Horizontal;
            m_collider.size = new Vector2(12.03f, 6.69f);
            m_collider.offset = new Vector2(-1.3f, -3.98f);
            portrait.CrossFade(Animation_Crawl_Start, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
            portrait.CrossFadeQueued(Animation_Crawl);
            PlayerManager.instance.isFreeToDoAction = false;
            PlayerManager.instance.CanWalkOnStairs = false;
            return;
        }
        if (!Crawling)
        {
            Crawling = !portrait.IsPlaying(Animation_Crawl_Start);
            SlowDown(16);
        }
    }

    void LeaveCrawlState()
    {
        if (m_collider.direction == CapsuleDirection2D.Horizontal)
        {
            m_collider.direction = CapsuleDirection2D.Vertical;
            m_collider.size = new Vector2(3.36f, 12.03f);
            m_collider.offset = new Vector2(0.3f, -1.25f);
        }
    }

    void StumbleState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Stumble; setIsFirstFrame(true); return; }

        if (isFirstFrame)
        {
            isFirstFrame = false;
            portrait.CrossFade(Animation_Stumble);
            portrait.SetAnimationSpeed(Animation_Stumble, 2);
            StartCoroutine(nameof(WaitForStandUp));
            if (PlayerManager.isTaking)
            {
                PlayerManager.isTaking = false;
                if (Pickable.held != null)
                {
                    Pickable.held.Throw(Vector2.zero);
                }
            }
            PlayerManager.instance.isFreeToDoAction = false;
            m_collider.direction = CapsuleDirection2D.Horizontal;
            m_collider.size = new Vector2(14.22f, 5.03f);
            m_collider.offset = new Vector2(-2.1f, -4.69f);
            rig.velocity = new Vector2(orient ? Stumble_Force.x : -Stumble_Force.x, Stumble_Force.y);
            PlayerManager.instance.CanWalkOnStairs = false;
            OnStumble?.Invoke();
            return;
        }
        SlowDown(8);

        if (!portrait.IsPlaying(Animation_Stumble) && !portrait.IsPlaying(Animation_Crawl_StandUp))
        {
            isSprinting = false;
            isFullSpeed = false;
            PlayerManager.instance.isFreeToDoAction = true;
            IdleState(true);
            m_collider.direction = CapsuleDirection2D.Vertical;
            m_collider.size = new Vector2(3.36f, 12.03f);
            m_collider.offset = new Vector2(0.3f, -1.25f);
            return;
        }
    }

    IEnumerator WaitForStandUp()
    {
        yield return new WaitForSeconds(Stumble_StandUp_time);
        portrait.CrossFade(Animation_Crawl_StandUp, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
    }

    void AttackState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Attack; setIsFirstFrame(true); return; }
        if (isFirstFrame)
        {
            isFirstFrame = false;
            rig.velocity = new Vector2(0, rig.velocity.y);
            Pickable.held.AttackRotation();
            portrait.CrossFade(Animation_Attack, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
            portrait.CrossFadeQueued(Animation_Idle_Weapon);
            return;
        }
        SlowDown(16);
        if (!portrait.IsPlaying(Animation_Attack))
        {
            PlayerManager.state = PlayerManager.StateCode.Idle;
            return;
        }
    }

    void SwimState(bool transition)
    {
        if (input.GetKeyDown(InputAction.Jump))
        {
            rig.AddForce(Vector2.up * JumpForceInWater);
        }
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Swim; setIsFirstFrame(true); return; }

        if (PlayerManager.instance.isInWater)
        {
            if (input.GetHorizonInput() != 0)
            {
                float speed = input.GetHorizonInput() * SpeedInWater;
                rig.AddForce(new Vector2(16 * (speed - rig.velocity.x), 0));
            }
            else if (Mathf.Abs(rig.velocity.x) < 2)
            {
                FloatState(true);
                return;
            }
            if (input.GetKey(InputAction.Down))
            {
                rig.velocity = new Vector2(rig.velocity.x, SwimDownSpeed);
            }
            rig.AddForce(new Vector2(-rig.velocity.x, (TerminalSpeedInWater - rig.velocity.y) * 4));
        }
        else
        {
            IdleState(true);
            rig.gravityScale = 1;
            return;
        }

        UpdateOrientation();
        if (isFirstFrame)
        {
            isFirstFrame = false;
            rig.gravityScale = 0;
            isSprinting = false;
            isFullSpeed = false;
            portrait.CrossFade(Animation_Swim);
        }
    }

    void FloatState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Float; setIsFirstFrame(true); return; }

        if (PlayerManager.instance.isInWater)
        {
            if (input.GetHorizonInput() != 0 || input.GetKeyDown(InputAction.Jump) || input.GetKey(InputAction.Down))
            {
                SwimState(true);
                return;
            }
            rig.AddForce(new Vector2(-rig.velocity.x, (TerminalSpeedInWater - rig.velocity.y) * 4));
        }
        else
        {
            IdleState(true);
            rig.gravityScale = 1;
            return;
        }

        if (isFirstFrame)
        {
            isFirstFrame = false;
            rig.gravityScale = 0;
            isSprinting = false;
            isFullSpeed = false;
            portrait.CrossFade(Animation_Float);
        }
    }

    bool isEnding = false;
    void PortIdleState(bool transition)
    {
        if (transition) 
        {
            if ((Portable.ported.transform.position.x - transform.position.x < 0) == orient)
            {
                return;
            }
            PlayerManager.state = PlayerManager.StateCode.Action_port_idle;
            setIsFirstFrame(true);
            return;
        }

        if (isEnding) return;
        if (input.GetKeyDown(InputAction.Interact))
        {
            StartCoroutine(nameof(EndPort));
        }
        if (isFirstFrame)
        {
            rig.velocity = Vector2.zero;
            isFirstFrame = false;
            portrait.CrossFade(Animation_Port_Start, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
            portrait.CrossFadeQueued(Animation_Port);
            PlayerManager.instance.CanWalkOnStairs = true;
            return;
        }
        if (rig.velocity.y < 0 && !PlayerManager.onGround)
        {
            LeavePortState();
            FallState(true);
        }
        if (input.GetHorizonInput() != 0 && (!portrait.IsPlaying(Animation_Port_Start)))
        {
            PortWalkState(true);
            return;
        }
        
    }

    void LeavePortState()
    {
        if (Portable.ported != null)
        {
            Portable.ported.PutDown();
        }
    }

    IEnumerator EndPort()
    {
        isEnding = true;
        portrait.CrossFade(Animation_Port_End, 0.3f, 0, apAnimPlayUnit.BLEND_METHOD.Interpolation, apAnimPlayManager.PLAY_OPTION.StopSameLayer, true);
        portrait.CrossFadeQueued(Animation_Idle);
        yield return new WaitForSeconds(Portable.ported.Putdown_time);
        Portable.ported.PutDown();
        yield return new WaitUntil(() => { return !portrait.IsPlaying(Animation_Port_End); });
        isEnding = false;
        PlayerManager.state = PlayerManager.StateCode.Idle;
    }

    void PortWalkState(bool transition)
    {
        if (transition) { PlayerManager.state = PlayerManager.StateCode.Action_port_walk; setIsFirstFrame(true); return; }

        if (Mathf.Abs(rig.velocity.x) < 0.1f && (input.GetHorizonInput() == 0))
        {
            portrait.CrossFade(Animation_Port);
            PlayerManager.state = PlayerManager.StateCode.Action_port_idle;
            return;
        }
        else
        {
            float speed = input.GetHorizonInput() * PortSpeed;
            rig.AddForce(new Vector2(16 * (speed - rig.velocity.x), 0));
        }

        if (rig.velocity.y < 0 && !PlayerManager.onGround)
        {
            LeavePortState();
            FallState(true);
        }

        if (isFirstFrame)
        {
            isFirstFrame = false;
            portrait.CrossFade(Animation_Port_Walk);
            PlayerManager.instance.CanWalkOnStairs = true;
        }
        UpdateOrientation();
    }

    IEnumerator _CollisionDetectionHelper()
    {
        while (true)
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

    public void LeaveAllState()
    {
        LeaveClimbState();
        LeavePortState();
        LeavePortState();
        LeaveCrawlState();
        LeaveMoveObjectState();
        if (PlayerManager.isTaking)
        {
            PlayerManager.isTaking = false;
            if (Pickable.held != null)
            {
                Pickable.held.Throw(Vector2.zero);
            }
        }
    }

    void UpdateIsSprinting()
    {
        if (PlayerManager.state == PlayerManager.StateCode.Walk)
        {
            WalkingTime += Time.deltaTime;
        }
        else
        {
            WalkingTime = 0;
        }
        if (PlayerManager.instance.ableToSprint != 0xff)
        {
            RunHint.SetActive(false);
            WalkingTime = 0;
            return;
        }
        if (WalkingTime > WalkTimeToRun)
        {
            if (!isSprinting)
            {
                RunHint.SetActive(true);
                if(orient)
                {
                    RunHint.transform.localScale = new Vector3(-Mathf.Abs(RunHint.transform.localScale.x), RunHint.transform.localScale.y, RunHint.transform.localScale.z);
                    RunHintText.text = input.GetKeyCode(InputAction.Right).ToString();
                }
                else
                {
                    RunHint.transform.localScale = new Vector3(Mathf.Abs(RunHint.transform.localScale.x), RunHint.transform.localScale.y, RunHint.transform.localScale.z);
                    RunHintText.text = input.GetKeyCode(InputAction.Left).ToString();
                }
            }
            else
            {
                RunHint.SetActive(false);
            }
            if (input.DoubleTapRight() || input.DoubleTapLeft())
            {
                isSprinting = true;
            }
        }
        else
        {
            RunHint.SetActive(false);
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
                WalkingTime = 0;
                return true;
            }
            else if (rig.velocity.x < -0.1f && orient)
            {
                gameObject.transform.localScale = new Vector3(Scale.x, Scale.y, Scale.z);
                orient = false;
                WalkingTime = 0;
                return true;
            }
        }
        return false;
    }

    void setIsFirstFrame(bool value)
    {
        isFirstFrame = value;
    }

    public float WalkOnStaris()
    {
        float speed = input.GetHorizonInput();
        if (!(PlayerManager.state == PlayerManager.StateCode.Idle || PlayerManager.state == PlayerManager.StateCode.Walk))
        {
            isSprinting = false;
            isFullSpeed = false;
            WalkingTime = 0;
            if (input.GetHorizonInput() == 0)
            {
                IdleState(true);
            }
            else
            {
                WalkState(true);
            }
        }
        return speed;
    }

    public void AssignEvent(EventType type, Action action)
    {
        switch(type)
        {
            case EventType.Run:
            {
                OnRunning += action;
                break;
            }
            case EventType.Stumble:
            {
                OnStumble += action;
                break;
            }
            case EventType.Jump:
            {
                OnJump += action;
                break;
            }
            case EventType.Land:
            {
                OnLanding += action;
                break;
            }
            case EventType.Die:
            {
                OnDie += action;
                break;
            }
        }
    }

    public static void Continue(bool value)
    {
        if(value)
        {
            Time.timeScale = 1;
            instance.portrait.SetPhysicEnabled(true);
        }
        else
        {
            Time.timeScale = 0;
            instance.portrait.SetPhysicEnabled(false);
        }
    }
}