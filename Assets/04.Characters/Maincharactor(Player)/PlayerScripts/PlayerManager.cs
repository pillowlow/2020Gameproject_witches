using System.Collections;
using Cinemachine;
using UnityEngine;

public class PlayerManager:MonoBehaviour
{
    public static PlayerManager instance = null;
    public GameObject player;
    public InputManager input;
    public CircleCollider2D ClimbRange;
    public LayerMask layer;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            player = gameObject;
            DontDestroyOnLoad(this.gameObject);
        }
        else if(instance != this)
        {
            Destroy(this.gameObject);
        }
    }
    public enum StateCode
    {
        Idle, Walk, Run, Brake, Crawl, Ride, Tic_Tac, Knock, Float, Jump, Fall,Climb,
        Action_pick, Action_push, Action_drag, Action_port,
        TakingHit, Stop, Flying, Die
    };
    public static bool isTaking = false;
    public static bool onGround = false;
    public static StateCode state = StateCode.Idle;
    
}
   

