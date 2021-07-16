using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public GameObject AttackParticle;
    public float AttackWaitingTime;
    public GameObject HorizontalAttackBox;
    public GameObject VerticalAttackBox;
    float lastAttackTime = -1;
    float lastHitTime = -1;
    Rigidbody2D rig;
    public enum AttackMode
    {
        idle, attack1, attack2,upAttack,
         flyAttack1, flyAttack1_connection
    }

    public AttackMode _attackMode;
    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        HorizontalAttackBox.SetActive(false);
        VerticalAttackBox.SetActive(false);
        _attackMode = AttackMode.idle;
        AttackParticleActive(false);
    }

    //Need to implement actual attack function
    private void Update()
    {
        if (PlayerManager.mode == PlayerManager.ModeCode.normal)
            NormalUpdate();
        else if (PlayerManager.mode == PlayerManager.ModeCode.transform)
            TransformUpdate();
        TurnOffAttackParticle();
    }
    

    private void NormalUpdate()
    {
        Attack_Input();
    }

    private void TransformUpdate()
    {
        Attack_Transform_Input();
    }

    //Player Attack
    private  void Attack_Input()
    {
        if (Input.GetButtonDown("Fire1") && CheckAttackable())
        {
            AttackModeSelector();
            AttackParticleActive();
            rig.velocity = new Vector2(0, rig.velocity.y);
            lastAttackTime = Time.time;
            StartCoroutine(SwingTime());
        }
    }

    private IEnumerator SwingTime()
    {
        yield return new WaitForSeconds(0.2f);
        HorizontalAttackBox.SetActive(false);
        VerticalAttackBox.SetActive(false);
    }

    private void AttackModeSelector()
    {
        if (Input.GetAxis("Vertical")>0)
        {
            _attackMode = AttackMode.upAttack;
            VerticalAttackBox.SetActive(true);
        }
        else
        {
            HorizontalAttackBox.SetActive(true);
        }
        switch (_attackMode)
        {
            case AttackMode.attack1:
                Attack2();
                _attackMode = AttackMode.attack2;
                break;
            case AttackMode.attack2:
                Attack1();
                _attackMode = AttackMode.attack1;
                break;
            case AttackMode.upAttack:
                Attack1();
                _attackMode = AttackMode.idle;
                break;
            default:
                Attack1();
                _attackMode = AttackMode.attack1;
                break;
        }
    }

    private void Attack_Transform_Input()
    {
        if (Input.GetButtonDown("Fire1") && CheckAttackable())
        {
            Attack1();
            lastAttackTime = Time.time;
            if (PlayerManager.state == PlayerManager.StateCode.Flying || PlayerManager.state == PlayerManager.StateCode.Falling)
                _attackMode = AttackMode.flyAttack1;
            else
                _attackMode = AttackMode.attack1;
        }
    }

    //Check if Player is able to attack
    private bool CheckAttackable()
    {
        float dtime = Time.time - lastAttackTime;
        if (dtime<AttackWaitingTime) return false;
        switch (PlayerManager.state)
        {
            case PlayerManager.StateCode.Idle:
            case PlayerManager.StateCode.Moving:
            case PlayerManager.StateCode.Jumping:
            case PlayerManager.StateCode.Flying:
            case PlayerManager.StateCode.Falling:
                return true;
            default:
                return false;
        }
        
    }

    //Animation things
    public void Attack(int type)
    {
        
    }

    public void AttackEnd()
    {
        
    }

    public void AttackParticleActive(bool b = true)
    {
        if (AttackParticle == null)
            return;
        if (b) AttackParticle.gameObject.SetActive(b);
        else AttackParticle.gameObject.SetActive(b);
    }
    void TurnOffAttackParticle()
    {
        if(PlayerManager.state == PlayerManager.StateCode.TakingHit)
            AttackParticleActive(false);
    }

    void Attack1()
    {
        //DB.animation.FadeIn(anim[(int)attack1], 0.2f, 1);
        AnimationControl.instance.attack1.Play(0.2f,false);
        _attackMode = AttackMode.attack1;
    }

    void Attack2()
    {

        //DB.animation.FadeIn(anim[(int)attack2], 0.2f, 1);
        AnimationControl.instance.attack2.Play(0.2f, false);
        _attackMode = AttackMode.attack2;

    }
}
