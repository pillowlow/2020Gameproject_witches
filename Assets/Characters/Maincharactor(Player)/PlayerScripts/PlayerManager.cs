using System.Collections;
using Cinemachine;
using UnityEngine;
/// <summary>
/// Stores informations about the main character.
/// </summary>
/// <remarks>Uses singleton</remarks>
public class PlayerManager:MonoBehaviour
{
    public static PlayerManager instance = null;
    /// <value>GameObject <c>player</c>:<br></br>
    /// The reference to the main character.
    /// </value>
    [HideInInspector] public GameObject player;
    /// <value>InputManager <c>input</c>:<br></br>
    /// The only input manager in whole game.
    /// </value>
    public InputManager input;
    /// <value>CircleCollider2D <c>HandRange</c>:<br></br>
    /// The circle collider representing the range of the hands.
    /// </value>
    public CircleCollider2D HandRange;
    /// <value>LayerMask <c>layer</c>:<br></br>
    /// Player layer.
    /// </value>
    public LayerMask layer;
    /// <value>Transform <c>RightHand</c>:<br></br>
    /// The reference to the transform of the socket on the right hand.
    /// </value>
    public Transform RightHand;
    /// <value>Transform <c>LeftHand</c>:<br></br>
    /// The reference to the transform of the socket on the left hand.
    /// </value>
    public Transform LeftHand;
    /// <value>bool <c>isFreeToDoAction</c>:<br></br>
    /// Whether the main character is able to do actions except idle and walk.
    /// </value>
    [HideInInspector] public bool isFreeToDoAction = true;
    /// <value>bool <c>CanWalkOnStairs</c>:<br></br>
    /// Whether the main character is able to walk on stairs.
    /// </value>
    [HideInInspector] public bool CanWalkOnStairs = true;
    /// <value>byte <c>ableToSprint</c>:<br></br>
    /// The main character is able to sprint if and only if it equals to 0xff.<br></br>
    /// Each bit representing different conditions.For example, the first bit is off if the main character is walking on stairs.
    /// </value>
    [HideInInspector] public byte ableToSprint = 0xff;
    /// <value>float <c>Stamina</c>:<br></br>
    /// The main character's stamina. Range from 0 to 100.
    /// </value>
    [Range(0,100)] public float Stamina = 100;
    /// <value>bool <c>isInWater</c>:<br></br>
    /// Whether the main character is in water.
    /// </value>
    [HideInInspector] public bool isInWater = false;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            player = gameObject;
            input = new InputManager();
            DontDestroyOnLoad(gameObject);
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// State codes of the player movement state machine.
    /// </summary>
    public enum StateCode
    {
        Idle, Walk, Run, Brake, Crawl, Attack, Float, Jump, Fall, Climb, Stumble,
        Action_pick, Action_move_object, Action_port_idle, Action_port_walk, Swim, Swing,
        Die,Reborn, None
    };

    /// <value>bool <c>isTaking</c>:<br></br>
    /// Whether the main character is holding weapons or objects.
    /// </value>
    public static bool isTaking = false;
    /// <value>bool <c>onGround</c>:<br></br>
    /// Whether the main character is standing on the ground.
    /// </value>
    public static bool onGround = false;
    /// <value>StateCode <c>state</c>:<br></br>
    /// The current movement state of the main character.
    /// </value>
    public static StateCode state = StateCode.Idle;

    /// <summary>
    /// For Debugging
    /// </summary>
    public static void SetState(StateCode code)
    {
        state = code;
    }
    
}
   

