using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed;
    public float flyForce;

    [Header("Attack")]
    public float attackWaitingTime;

    CharacterController2D controller;

    Rigidbody2D rig;
    Animator anim;

    bool jump;
    bool fly = false;

    float lastAttactTime;
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController2D>();
    }


    void Update()
    {
        Movement();
        Attack0();
        AnimationControl();
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");

        jump = false;

        if (Input.GetButtonDown("Jump"))
        {
            if (PlayerManager.state == 0)
            {
                jump = true;
                if (controller.GetOnGround()) anim.SetTrigger("Jump");
            }
            else fly = true;
            //fly = true;
        }

        controller.Jump(jump);

    }

    void Attack0()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if(Time.time - lastAttactTime > attackWaitingTime)
            {
                anim.SetTrigger("Attack0");
                lastAttactTime = Time.time;
            }
            
        }
    }

    void AnimationControl()
    {
        anim.SetFloat("RunSpeed", Mathf.Abs(rig.velocity.x));
        anim.SetFloat("JumpVelocity", rig.velocity.y);
        anim.SetBool("OnGround", controller.GetOnGround());

        if (fly)
        {
            anim.SetTrigger("Fly");
            fly = false;
            Fly();
        }

    }

    public void Fly()
    {
        rig.velocity = new Vector2(rig.velocity.x, 0);
        rig.AddForce(new Vector2(0, flyForce));
    }
}
