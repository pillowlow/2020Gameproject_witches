using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class BossCntr : MonoBehaviour
{
    [Header("Movement")]
    public GameObject walkParticle;
    public Transform walkPos;
    public float moveForce;
    public float jumpForce;
    int direction = -1;

    [Header("Player")]
    public GameObject player;
    public LayerMask playerLayerMask;

    [Header("Camera")]
    public CameraShake cameraShake;

    [Header("BossAi")]
    public float bossWaitingTime;
    public float stopRange;

    [Header("Attack")]
    public Transform[] snipePoints;
    public GameObject[] attackPoints;
    public float[] attackRange;
    public float snipeForce;

    [Header("Testing")]
    public bool testing;

    int state;
    float counter, timer;
    float distanceP;
    bool snipeable;
    Vector3 velocity__ = Vector3.zero;

    Rigidbody2D rig;
    Animator anim;

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), GetComponent<Collider2D>());

        state = 1;
        counter = 0;
        timer = 0;
        snipeable = false;
    }

    // Update is called once per frame
    void Update()
    {
        Timer();
        if(testing) Testing();
        else BossAI();
        
    }


    void BossAI()
    {
        distanceP = CheckDistance(transform.position, player.transform.position);

        switch (state)
        {
            case 0:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("idel"))
                {
                    timer = 0;
                    state = 1;
                }
                break;

            case 1:
                if(timer > bossWaitingTime)
                {
                    state = 2;
                }
                break;

            case 2:
                FacePlayer();
                state = 3;
                break;

            case 3:
                AttackSwitch();
                break;

            case 4:
                WalkToPlayer();
                break;

            case 5:
                ReachToPlayer();
                break;

            case 6:
                anim.SetTrigger("attack0");
                state = 0;
                break;

            case 7:
                anim.SetTrigger("Jump");
                state = 8;
                break;

            case 8:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("jump_mid") &&
                    Physics2D.Raycast(transform.position, new Vector3(0, 1, 0), 0.1f) &&
                    rig.velocity.y < 0.01f)
                {
                    anim.SetTrigger("SJump");
                    CameraShake_(0.4f);
                    JumpAttack();
                    state = 0;
                }
                break;

            case 9:
                anim.SetTrigger("Snipe");
                state = 10;
                snipeable = false;
                break;

            case 10:
                if (snipeable == true)
                {
                    state = 0;
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

    Vector3 CheckFaceSnipePoint()
    {
        for (int i = 0; i < snipePoints.Length; i++)
        {
            if ((snipePoints[i].position.x - transform.position.x) * direction > 0)
                return new Vector3(snipePoints[i].position.x, transform.position.y, transform.position.z);
        }
        return Vector3.zero;
    }

    void AttackSwitch()
    {
        float rand = Random.Range(0f,1f);
        if(distanceP < 6.5f)
        {
            if (rand < 0.5f) state = 9;
            else state = 4;
        }
        else
        {
            if (rand < 0.3f) state = 4;
            else if (rand < 0.43f) state = 9;
            else state = 7;
        }
    }

    void WalkToPlayer()
    {
        if(distanceP > stopRange)
        {
            anim.SetTrigger("Walk");
            state = 5;
        }
        else
        {
            state = 6;
        }
    }

    void ReachToPlayer()
    {
        if (distanceP <= stopRange)
        {
            anim.SetTrigger("SWalk");
            state = 6;
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
        StartCoroutine(cameraShake.Shake(0.1f, magnitude));
    }

    public void Jump()
    {
        float distance = CheckDistance(transform.position, player.transform.position);
        float JVelocity = 22.5f * distance;

        rig.AddForce(new Vector3(direction * JVelocity, jumpForce, 0));
    }

    public void JumpAttack()
    {
        if (Physics2D.Raycast(attackPoints[0].transform.position, new Vector3(direction, 0, 0), attackRange[0], playerLayerMask))
        {
            //PlayerDamage();
        }
    }
    public void Attack0()
    {
        if (Physics2D.Raycast(attackPoints[0].transform.position, new Vector3(direction,0,0), attackRange[0], playerLayerMask))
        {
            //StartCoroutine(cameraShake.Shake(0.1f, 0.5f));
        }
    }

    public void Snipe()
    {
        rig.AddForce(new Vector2(snipeForce * direction, 0));
        snipeable = true;
    }

    public void SnipeEnd()
    {
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

        for (int i=0; i < snipePoints.Length; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(snipePoints[i].position, 0.5f);
        }
    }
}
