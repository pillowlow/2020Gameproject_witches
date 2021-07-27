using System.Collections;
using Cinemachine;
using UnityEngine;

public class PlayerManager:MonoBehaviour
{
    public static PlayerManager instance = null;
    public GameObject player;
    public InputManager input;
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
        Idle, Walk, Run, Brake, Crawl, Ride, Tic_Tac, Knock, Float, Jump, Fall,
        Take_cast, Take_push, Take_drag, Take_port,
        Action_cast, Action_push, Action_drag, Action_port,
        TakingHit, Stop, Flying, Die
    };
    public static bool onGround = false;
    public static StateCode state = StateCode.Idle;
    
}
   

