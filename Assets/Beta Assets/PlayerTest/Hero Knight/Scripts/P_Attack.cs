using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Attack : MonoBehaviour
{
    [Header("Attack")]
    public float attackWaitingTime;
    public Transform[] attackPoints;
    public float[] attackRange;

    float lastAttactTime;

    Rigidbody2D rig;
    Animator anim;

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (PlayerManager.mode == PlayerManager.ModeCode.normal)
            NormalUpdate();
        else if (PlayerManager.mode == PlayerManager.ModeCode.transform)
            TransformUpdate();
    }

    void NormalUpdate()
    {
        Attack_Input();
    }

    void TransformUpdate()
    {
        Attack_Transform_Input();
    }

    void Attack_Input()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            float dtime = Time.time - lastAttactTime;
            if (CheckAttackable())
            {
                anim.SetTrigger("Attack1");
                rig.velocity = new Vector2(0, rig.velocity.y);
                lastAttactTime = Time.time;
                PlayerManager.state = PlayerManager.StateCode.attack1;
            }
            if (PlayerManager.state == PlayerManager.StateCode.attack1_connection)
            {
                anim.SetTrigger("Attack2");
                lastAttactTime = Time.time;
                PlayerManager.state = PlayerManager.StateCode.attack2;
            }

        }
    }
    bool CheckAttackable()
    {
        bool attackable = false;
        if (PlayerManager.state == PlayerManager.StateCode.idel) attackable = true;
        else if (PlayerManager.state == PlayerManager.StateCode.moving) attackable = true;

        if(PlayerManager.mode == PlayerManager.ModeCode.transform)
        {
            if (PlayerManager.state == PlayerManager.StateCode.flying) attackable = true;
            else if(PlayerManager.state == PlayerManager.StateCode.falling) attackable = true;
        }

        return attackable;

    }

    public void Attack(int type)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackPoints[type].position, attackRange[type], PlayerManager.instance.enemyLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            EnemyManager enemy = colliders[i].gameObject.GetComponent<EnemyManager>();
            enemy.Damaged(PlayerManager.damage);
        }

        if (type == 1)
            PlayerManager.state = PlayerManager.StateCode.attack1_connection;
        else if (type == 2)
            PlayerManager.state = PlayerManager.StateCode.attack2_connection;
        else if (type == 3)
            PlayerManager.state = PlayerManager.StateCode.flyAttack1_connection;
    }

    public void Attack_Transform(int type)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackPoints[type].position, attackRange[type], PlayerManager.instance.enemyLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            EnemyManager enemy = colliders[i].gameObject.GetComponent<EnemyManager>();
            enemy.Damaged(PlayerManager.damage);
        }

        if (PlayerManager.state == PlayerManager.StateCode.flyAttack1)
            PlayerManager.state = PlayerManager.StateCode.flyAttack1_connection;
        else if (PlayerManager.state == PlayerManager.StateCode.attack1)
            PlayerManager.state = PlayerManager.StateCode.attack1_connection;
    }

    public void AttackEnd()
    {
        bool changeState = false;
        if (PlayerManager.state == PlayerManager.StateCode.attack1_connection) changeState = true;
        else if (PlayerManager.state == PlayerManager.StateCode.attack2_connection) changeState = true;
        else if (PlayerManager.state == PlayerManager.StateCode.flyAttack1_connection) changeState = true;

        Debug.Log("222");

        if (changeState) PlayerManager.state = PlayerManager.StateCode.idel;
    }

    void Attack_Transform_Input()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            float dtime = Time.time - lastAttactTime;
            if (CheckAttackable())
            {
                anim.SetTrigger("Attack1");
                lastAttactTime = Time.time;

                if(PlayerManager.state == PlayerManager.StateCode.flying || PlayerManager.state == PlayerManager.StateCode.falling)
                    PlayerManager.state = PlayerManager.StateCode.flyAttack1;
                else 
                    PlayerManager.state = PlayerManager.StateCode.attack1;
            }
        }
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
