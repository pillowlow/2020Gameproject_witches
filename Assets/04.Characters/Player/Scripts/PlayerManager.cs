using System;
using System.Collections;
using System.Collections.Generic;
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
        var pMovement = player.GetComponent<P_Movement>();
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

    public static bool isTalking = false;
    public static bool talkable = false;
    public static bool isFlying = false;
    public static bool isJumping = false;
    public static bool moveable = true;
    public static bool onGround = false;
    public static int talk_man;

    public static ModeCode mode = ModeCode.normal;
    public static StateCode state = StateCode.idle;
    public static int hp = 100;
    public static int damage = 5;
    public static int sanityValue = 100;


    public enum StateCode {
        idle, die, moving, jumping, falling, flying, takingHit, attack1, 
        attack1_connection , attack2 , attack2_connection, flyAttack1,
        flyAttack1_connection
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
        static int lvDamage;
        static int equipDamage;

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

    public static void TakeDamage(int damage)
    {
        int hp_ = hp;

        if (state != StateCode.takingHit && state != StateCode.die)
        {
            hp_ = (hp_ - damage > 0) ? hp_ - damage : 0;
            state = PlayerManager.StateCode.takingHit;

            if (mode == ModeCode.normal) instance.anim.SetTrigger("TakeHit");
            else instance.anim_t.SetTrigger("TakeHit");

        }

        if(hp_ == 0 && state != StateCode.die)
        {
            if(mode != ModeCode.normal)
            {
                instance.player_transform.GetComponent<P_Transform>().Transform(ModeCode.normal);
            }

            instance.anim.SetBool("Die", true);
            instance.anim.SetTrigger("DieT");
            state = StateCode.die;
        }

        hp = hp_;
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

