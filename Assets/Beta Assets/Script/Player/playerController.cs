using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private playerUnit Unit;

    public AudioClip jumpsound;

    public bool OnGround;
    public Transform feetPos;
    public Vector2 feetSize;
    public LayerMask Ground;

    private float jumpTimeCounter;
    
    public GameObject attackPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        Unit = GetComponent<playerUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        OnGround = Physics2D.OverlapBox(feetPos.position, feetSize,0.0f, Ground);
        jump();
        movement();
    }

    private void movement()
    {
        if (Input.GetKey(KeyCode.D))
        {
            //transform.Translate(new Vector2(speed, 0) * Time.deltaTime);
            rb.velocity = new Vector2(Unit.speed, rb.velocity.y);
            if(transform.rotation != Quaternion.Euler(0, 0, 0))
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            //spriteRenderer.flipX = true;
            //attackPoint.transform.position=new Vector2(0.102f,attackPoint.transform.position.y);
            animator.SetInteger("AnimState",2);
        }
        else
        {
            if (Input.GetKey(KeyCode.A))
            {
                rb.velocity = new Vector2(-Unit.speed, rb.velocity.y);
                if(transform.rotation != Quaternion.Euler(0, 180, 0))
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                //spriteRenderer.flipX = false;
                //attackPoint.transform.position=new Vector2(-0.102f,attackPoint.transform.position.y);
                animator.SetInteger("AnimState",2);
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                animator.SetInteger("AnimState", 0);
            }
        }
    }

    private void jump()
    {
        if (OnGround)
        {
            animator.SetBool("Grounded", true);
            if (Input.GetKeyDown(KeyCode.K))
            {
                jumpTimeCounter = Unit.jumpTime;
                audioSource.PlayOneShot(jumpsound);
            }
        }
        else
        {
            animator.SetBool("Grounded", false);
        }
        if (Input.GetKey(KeyCode.K))
        {
            if (jumpTimeCounter > 0)
            {
                rb.velocity = Vector2.up * Unit.jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
        }
        else
        {
            jumpTimeCounter = 0;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(feetPos.position,feetSize);
    }
}
