using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public GameObject attackParticle;
    public float attackWaitingTime;
    public Transform[] attackPoints;
    public float[] attackRange;

    float lastAttackTime;

    Rigidbody2D rig;
    Animator anim;
  
    public enum AttackMode
    {
        idle, attack1, attack1_connection, attack2,
        attack2_connection, flyAttack1, flyAttack1_connection
    }

    private AttackMode _attackMode;
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        _attackMode = AttackMode.idle;
        AttackParticleActive(false);
    }

    //Need to implement actual attack function
    void Update()
    {
        if (PlayerManager.mode == PlayerManager.ModeCode.normal)
            NormalUpdate();
        else if (PlayerManager.mode == PlayerManager.ModeCode.transform)
            TransformUpdate();

        TureOffAttackParticle();

    }

    void TureOffAttackParticle()
    {
        if(PlayerManager.state == PlayerManager.StateCode.takingHit)
            AttackParticleActive(false);
    }

    void NormalUpdate()
    {
        Attack_Input();
    }

    void TransformUpdate()
    {
        Attack_Transform_Input();
    }

    //Player Attack
    void Attack_Input()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            float dtime = Time.time - lastAttackTime;
            if (dtime<attackWaitingTime) return;
            if (CheckAttackable())
            {
                anim.SetTrigger("Attack1");
                AttackParticleActive();
                rig.velocity = new Vector2(0, rig.velocity.y);
                lastAttackTime = Time.time;
                _attackMode = AttackMode.attack1;
            }
            if (_attackMode == AttackMode.attack1_connection)
            {
                anim.SetTrigger("Attack2");
                lastAttackTime = Time.time;
                _attackMode = AttackMode.attack2;
            }

        }
    }

    void Attack_Transform_Input()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            float dtime = Time.time - lastAttackTime;
            if (dtime<attackWaitingTime) return;
            if (CheckAttackable())
            {
                anim.SetTrigger("Attack1");
                lastAttackTime = Time.time;

                if (PlayerManager.state == PlayerManager.StateCode.flying || PlayerManager.state == PlayerManager.StateCode.falling)
                    _attackMode = AttackMode.flyAttack1;
                else
                    _attackMode = AttackMode.attack1;
            }
        }
    }

    //Check if Player is able to attack
    bool CheckAttackable()
    {
        switch (PlayerManager.state)
        {
            case PlayerManager.StateCode.idle:
            case PlayerManager.StateCode.moving:
            case PlayerManager.StateCode.jumping:
            case PlayerManager.StateCode.flying:
            case PlayerManager.StateCode.falling:
                return true;
            default:
                return false;
        }
        
    }

    //Animation things
    public void Attack(int type)
    {
        if (type == 1)
            _attackMode = AttackMode.attack1_connection;
        else if (type == 2)
            _attackMode = AttackMode.attack2_connection;
        else if (type == 3)
            _attackMode = AttackMode.flyAttack1_connection;
    }

    public void AttackEnd()
    {
        bool changeState = false;
        if (_attackMode == AttackMode.attack1_connection) changeState = true;
        else if (_attackMode == AttackMode.attack2_connection) changeState = true;
        else if (_attackMode == AttackMode.flyAttack1_connection) changeState = true;

        if (changeState)
        {
            PlayerManager.state = PlayerManager.StateCode.idle;
            _attackMode = AttackMode.idle;
        }
        if(PlayerManager.mode == PlayerManager.ModeCode.normal)
            AttackParticleActive(false);

    }

    public void AttackParticleActive(bool b = true)
    {
        if (attackParticle == null)
            return;
        if (b) attackParticle.gameObject.SetActive(b);
        else attackParticle.gameObject.SetActive(b);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < attackPoints.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoints[i].position, attackRange[i]);
        }
    }
}
