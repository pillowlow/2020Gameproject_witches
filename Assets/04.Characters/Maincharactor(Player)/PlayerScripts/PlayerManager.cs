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
        }
        else if(instance != this)
        {
            Destroy(this.gameObject);
        }
    }
    public enum StateCode
    {
        Idle, Die, Walking, Running, Braking, Jumping, Falling, TakingHit, Stop, Flying, Crawling
    };
    public static bool onGround = false;
    public static StateCode state = StateCode.Idle;
    
}
   

