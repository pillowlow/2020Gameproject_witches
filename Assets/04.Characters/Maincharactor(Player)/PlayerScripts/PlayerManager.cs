using System.Collections;
using Cinemachine;
using UnityEngine;

public class PlayerManager:MonoBehaviour
{
    public static PlayerManager instance = null;
    [HideInInspector] public GameObject player;
    public InputManager input;
    public CircleCollider2D HandRange;
    public LayerMask layer;
    public Transform RightHand;
    public Transform LeftHand;
    [HideInInspector] public bool isFreeToDoAction = true;
    [HideInInspector] public bool CanWalkOnStairs = true;
    [Range(0,100)] public float Stamina = 100;
    public bool isInWater = false;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            player = gameObject;
            input = new InputManager();
            DontDestroyOnLoad(this.gameObject);
        }
        else if(instance != this)
        {
            Destroy(this.gameObject);
        }
    }
    public enum StateCode
    {
        Idle, Walk, Run, Brake, Crawl, Attack, Float, Jump, Fall, Climb, Stumble,
        Action_pick, Action_move_object, Action_port_idle, Action_port_walk, Swim, Swing,
        Die,Reborn, Stop, None
    };
    public static bool isTaking = false;
    public static bool onGround = false;
    public static StateCode state = StateCode.Idle;
    
}
   

