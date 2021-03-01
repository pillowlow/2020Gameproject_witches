using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossCntr : MonoBehaviour
{
    
    public GameObject attackParticle;
    public GameObject slashEffect;
    [Header("Movement")]
    public GameObject walkParticle;
    public Transform walkPos;
    public float moveForce;
    public float jumpForce;
    int direction = -1;

    [Header("Player")]
    public LayerMask playerLayerMask;

    [Header("Camera")]
    public CameraShake cameraShake;

    [Header("BossAi")]
    public float bossWaitingTime;
    public float stopRange;

    [Header("Attack")]
    public GameObject[] attackPoints;
    public float[] attackRange;
    public float snipeForce;
    
    
    [Header("Testing")]
    public bool testing;
    int originHp;
    float counter, timer;
    float distanceP;
    bool snipeable;
    Vector3 velocity__ = Vector3.zero;

    GameObject player, player_, player_T;
    Rigidbody2D rig;
    Animator anim;
    EnemyManager eManager;

    enum BossMode
    {
        Idle,CalWaiting,FacePlayer,AttackSwitch,WalkToPlayer,
        ReachToPlayer,Attack0,Jump,JumpAttack,TriggerSnipe,Snipe
    }
    BossMode state;
    
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        //Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), GetComponent<Collider2D>());

        player_ = PlayerManager.instance.player;
        player_T = PlayerManager.instance.player_transform;

        state = BossMode.CalWaiting;
        counter = 0;
        timer = 0;
        snipeable = false;        
        eManager = GetComponent<EnemyManager>();
        originHp = eManager.GetHp();
        AttackParticleActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerManager.mode == PlayerManager.ModeCode.normal) player = player_;
        else player = player_T;

        Timer();
        if(eManager.GetHp() <= 0)
        {
            if(eManager.isDie != true)
            {
                eManager.isDie = true;
                Destroy(GetComponent<Collider2D>());
                Destroy(rig);
                anim.SetTrigger("Die");
            }
        }
        else BossAI();
        
    }


    void BossAI()
    {
        distanceP = CheckDistance(transform.position, player.transform.position);
        switch (state)
        {
            case BossMode.Idle:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("idle"))
                {
                    timer = 0;
                    state = BossMode.CalWaiting;
                    counter = 0;
                    AttackParticleActive(false);
                }
                break;

            case BossMode.CalWaiting:
                float bossWaitingTime_ = bossWaitingTime;
                float hpEff = GetComponent<EnemyManager>().GetHp() / originHp;
                AttackParticleActive(false);
                if (hpEff <= 0.3) bossWaitingTime_ = 1;
                else if (hpEff <= 0.5) bossWaitingTime = 2;

                if (timer > bossWaitingTime_)
                {
                    state = BossMode.FacePlayer;
                }
                break;

            case BossMode.FacePlayer:
                FacePlayer();
                state = BossMode.AttackSwitch;
                AttackParticleActive(false);
                break;

            case BossMode.AttackSwitch:
                AttackSwitch();
                AttackParticleActive(false);
                break;

            case BossMode.WalkToPlayer:
                WalkToPlayer();
                AttackParticleActive(false);
                break;

            case BossMode.ReachToPlayer:
                ReachToPlayer();
                AttackParticleActive(false);
                break;

            case BossMode.Attack0:
                anim.SetTrigger("attack0");
                state = BossMode.Idle;
                AttackParticleActive(true);
                break;

            case BossMode.Jump:
                anim.SetTrigger("Jump");
                state = BossMode.JumpAttack;
                AttackParticleActive(false);
                break;
                

            case BossMode.JumpAttack:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("jump_mid") &&
                    Physics2D.Raycast(transform.position, new Vector3(0, 1, 0), 0.1f) &&
                    rig.velocity.y < 0.01f)
                {
                    anim.SetTrigger("SJump");
                    CameraShake_(0.4f);
                    JumpAttack();
                    AttackParticleActive(false);
                    state = BossMode.Idle;
                }
                break;

            case BossMode.TriggerSnipe:
                anim.SetTrigger("Snipe");
                state = BossMode.Snipe;
                AttackParticleActive(true);
                snipeable = false;
                break;
               

            case BossMode.Snipe:
                if (snipeable == true)
                {
                    Attack0();
                    AttackParticleActive(true);
                    Instantiate(slashEffect, (transform.position), transform.rotation);
                }
                break;

            default:
                break;
        }
    }

    float CheckDistance(Vector3 a, Vector3 b, int mode = 0)
    {
        return (mode == 1)? Mathf.Abs(a.x - b.x) : Vector2.Distance(a, b);
    }
    void Timer()
    {
        timer += Time.deltaTime;
    }

    void FacePlayer()
    {
        if (transform.position.x > player.transform.position.x) direction = -1;
        else direction = 1;

        if (direction == 1)
            transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        else transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    void AttackSwitch()
    {
        float rand = Random.Range(0f,1f);
        if(distanceP < 6.5f)
        {
            if (rand < 0.4f) state = BossMode.TriggerSnipe;
            else state = BossMode.WalkToPlayer;
        }
        else
        {
            if (rand < 0.3f) state = BossMode.WalkToPlayer;
            else if (rand < 0.43f) state = BossMode.TriggerSnipe;
            else state = BossMode.Jump;
        }
    }

    void WalkToPlayer()
    {
        if(distanceP > stopRange)
        {
            counter = timer;
            anim.SetTrigger("Walk");
            state = BossMode.ReachToPlayer;
        }
        else
        {
            state = BossMode.Attack0;
        }
    } 
    void AttackParticleActive(bool b = true)
    {
        if (b) attackParticle.gameObject.SetActive(b);
        else attackParticle.gameObject.SetActive(b);
    }
    void ReachToPlayer()
    {
        if (distanceP <= stopRange)
        {
            anim.SetTrigger("SWalk");
            state = BossMode.Attack0;
        }
        else if (counter + 3.5 < timer)
        {
            anim.SetTrigger("SWalk");
            FacePlayer();
            state = BossMode.Jump;
        }
    }
    

    #region Animation_Events
    public void One_Step_Walk()
    {
        GameObject eff = Instantiate(walkParticle);
        eff.transform.position = walkPos.position;
        eff.GetComponent<ParticleSystem>().Play();

        rig.AddForce(new Vector2(direction * moveForce, 0));
    }

    public void Stop_Walking()
    {
        rig.velocity = Vector2.zero;
    }

    public void CameraShake_(float magnitude)
    {
        StartCoroutine(cameraShake.Shake(0.15f, magnitude));
    }

    public void Jump()
    {
        float distance = CheckDistance(transform.position, player.transform.position);
        float JVelocity = 22.5f * distance;

        rig.AddForce(new Vector3(direction * JVelocity, jumpForce, 0));
    }

    public void JumpAttack()
    {
        AttackParticleActive(true);
        if (Physics2D.Raycast(attackPoints[1].transform.position, new Vector3(direction, 0, 0), attackRange[0], playerLayerMask))
        {
            int damage = GetComponent<EnemyManager>().damage;
            
            if (PlayerManager.state != PlayerManager.StateCode.TakingHit)
                
                PlayerManager.TakeDamage(damage,transform);
        }
    }
    public void Attack0()
    {
        AttackParticleActive(true);
        if (Physics2D.Raycast(attackPoints[0].transform.position, new Vector3(direction,0,0), attackRange[0], playerLayerMask))
        {
            int damage = GetComponent<EnemyManager>().damage;
            AttackParticleActive(true);
            PlayerManager.TakeDamage(damage,transform);
        }
    }

    public void Snipe()
    {
        rig.AddForce(new Vector2(snipeForce * direction, 0));
        AttackParticleActive(true);
        snipeable = true;
    }

    public void SnipeEnd()
    {
        state = BossMode.Idle;
        rig.velocity = Vector2.zero;
    }

    #endregion

    void Testing()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FacePlayer();
            anim.SetTrigger("testing");
        }
            
        if (Input.GetKeyDown(KeyCode.S))
            StartCoroutine(cameraShake.Shake(0.1f, 0.2f));


        /*if (anim.GetCurrentAnimatorStateInfo(0).IsName("jump_mid") &&
            Physics2D.Raycast(transform.position, new Vector3(0, 1, 0), 0.1f) &&
            rig.velocity.y < 0.01f)
        {
            anim.SetTrigger("SJump");
            CameraShake_(0.2f);
        }*/
            
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, stopRange);

        for(int i=0; i < attackRange.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(attackPoints[i].transform.position, attackPoints[i].transform.position + new Vector3(attackRange[i] * direction, 0, 0));
        }
    }
}
