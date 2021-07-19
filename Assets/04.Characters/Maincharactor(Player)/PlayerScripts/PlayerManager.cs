using System.Collections;
using Cinemachine;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton
    public static PlayerManager instance;
    
    private CinemachineVirtualCamera _VirtualCamera = null;
    private CinemachineFramingTransposer _cinemachineComposer = null;
    private void Awake()
    {
        instance = this;
        _rigidbody2D = instance.player.GetComponent<Rigidbody2D>();
        anim = instance.player.GetComponent<Animator>();
        anim_t = instance.player_transform.GetComponent<Animator>();
        
        
    }

    private void Start()
    {
        var pMovement = player.GetComponent<PlayerMovement>();
        pMovement.OnJump += () =>
        {
             if (_cinemachineComposer != null)
            {
                _cinemachineComposer.m_DeadZoneHeight = 1.0f;
            }
        };
        pMovement.OnLanding += () =>
        {
            if (_cinemachineComposer != null)
            {
                _cinemachineComposer.m_DeadZoneHeight = 0f;
            }
        };
    }

    #endregion

    public GameObject player;
    public GameObject player_transform;
    public LayerMask enemyLayer;

    private Rigidbody2D _rigidbody2D;
    public Rigidbody2D CurrentRigidbody2D
    {
        get => _rigidbody2D;
    }
    Animator anim, anim_t;
    
    public static bool onGround = false;
    public static ModeCode mode = ModeCode.normal;
    public static StateCode state = StateCode.Idle;
    public static int hp = 100;
    public static int damage = 5;
    public static int sanityValue = 100;


    public enum StateCode {
        Idle, Die, Walking, Running, Jumping, Falling, TakingHit, Stop, Flying
    };

    public enum ModeCode{
        normal, transform
    };

    public static class Exp
    {
        static int lv = 0;
        static int value = 0;

        public static int GetValue()
        {
            return value;
        }
        public static int GetLv()
        {
            return lv;
        }
        public static void AssignValue(int val)
        {
            value = val;
        }
        public static void AssignLv(int val)
        {
            lv = val;
        }
    }

    public static class Damage
    {
        static int lvDamage=5;
        static int equipDamage=5;

        public static int GetDamage()
        {
            return lvDamage + equipDamage;
        }
        public static int GetLvDamage()
        {
            return lvDamage;
        }
        public static int GetEquipDamage()
        {
            return equipDamage;
        }
        public static void AssignLvDamage(int damage)
        {
            lvDamage = damage;
        }
        public static void AssignEquipDamage(int damage)
        {
            equipDamage = damage;
        }
    }

    public static void TakeDamage(int damage,Transform enemy)
    {
        if(damage==0) return;
        int hp_ = hp;
        
        if (state != StateCode.TakingHit && state != StateCode.Die)
        {
            hp_ = (hp_ - damage > 0) ? hp_ - damage : 0;
            state = StateCode.TakingHit;

            if (mode == ModeCode.normal) instance.anim.SetTrigger("TakeHit");
            else instance.anim_t.SetTrigger("TakeHit");

            if (hp_ == 0)
            {
                if(mode != ModeCode.normal)
                {
                    instance.player_transform.GetComponent<PlayerTransform>().Transform(ModeCode.normal);
                }
                instance.anim.SetBool("Die", true);
                instance.anim.SetTrigger("DieT");
                state = StateCode.Die;
                instance._rigidbody2D.velocity = Vector2.zero;
            }
            else
            {
                Vector2 dir = (instance.player.transform.position - enemy.position).normalized;
                instance.StartCoroutine(TakeHit());
            }
            
        }
        
        hp = hp_;
    }

    static IEnumerator TakeHit()
    {
        yield return new WaitForSeconds(1);
        state = StateCode.Idle;
    }
    
    public static void AssignSanityValue(int value)
    {
        sanityValue += value;
    }
    
    
    public void SetPlayerCamera(CinemachineVirtualCamera camera)
    {
        _VirtualCamera = camera;
        //TODO need check player gameObject status is transform or not
        _VirtualCamera.Follow = player.transform;
        _cinemachineComposer = camera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    public void OnTransformFinish(GameObject player)
    {
        _VirtualCamera.Follow = player.transform;
    }
}

