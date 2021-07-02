using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnyPortrait;

public class Controll_Doll : MonoBehaviour
{
    public apPortrait portrait;
    public Rigidbody2D rg2D;

    private enum State
    {
        idle,
        Walk,
        Jump,
        Run,
    }
    private State state = State.idle;

    private bool isFirstFrame = false;
    private bool isGround = true;

    // Start is called before the first frame update
    void Start()
    {
        state = State.idle;
        isFirstFrame = true;
        isGround = true;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.idle:
                Updateidle();
                break;

            case State.Walk:
                UpdateWalk();
                break;

            case State.Jump:
                UpdateJump();
                break;

            case State.Run:
                UpdateRun();
                break;
        }

    }

    private void Updateidle()
    {
        if (isFirstFrame)
        {
            portrait.CrossFade("idle",0.2f);
            isFirstFrame = false;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            ChangeState(State.Walk);
        }
        else if (Input.GetKey(KeyCode.W) && isGround==true)
        {
            ChangeState(State.Jump);
        }
    }

    private void UpdateWalk()
    {
        if (isFirstFrame)
        {
            portrait.CrossFade("Walk1", 0.4f);
            isFirstFrame = false;
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                ChangeState(State.Run);
            if (Input.GetKey(KeyCode.W) && isGround)
                ChangeState(State.Jump);
            transform.Translate(new Vector3(-2f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                ChangeState(State.Run);
            if (Input.GetKey(KeyCode.W) && isGround)
                ChangeState(State.Jump);
            transform.Translate(new Vector3(2f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
        }
        else
        {
            ChangeState(State.idle);
        } 
        
    }

    private void UpdateRun()
    {
        if (isFirstFrame)
        {
            portrait.CrossFade("Run", 0.4f);
            isFirstFrame = false;
        }
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.W) && isGround)
                ChangeState(State.Jump);
            transform.Translate(new Vector3(-4f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else if (Input.GetKey(KeyCode.D)&& Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.W) && isGround)
                ChangeState(State.Jump);
            transform.Translate(new Vector3(4f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
        }
        else
        {
            if (!Input.GetKey(KeyCode.LeftShift))
                ChangeState(State.Walk);
            else
                ChangeState(State.idle);
        }

    }

    private void UpdateJump()
    {
        if (isFirstFrame)
        {
            isGround = false;
            portrait.CrossFade("jump", 0.1f);
            rg2D.AddForce(new Vector2(0, 50), ForceMode2D.Impulse);
            portrait.CrossFade("fall",0.1f);
            isFirstFrame = false;
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.Translate(new Vector3(-4f * Time.deltaTime, 0, 0));
            }
            else
                transform.Translate(new Vector3(-2f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            
            if(Input.GetKey(KeyCode.LeftShift))
            {
                transform.Translate(new Vector3(4f * Time.deltaTime, 0, 0));
            }
            else
                transform.Translate(new Vector3(2f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
        }


        if ((rg2D.velocity.y<=0)&&(isGround))
        {
            portrait.CrossFade("land",0.1f);
            if (Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.A))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    ChangeState(State.Run);
                else
                    ChangeState(State.Walk);
            }
            else
            {
                ChangeState(State.idle);
            }
        }

    }

    private void ChangeState(State nextState)
    {
        if (state != nextState)
        {
            state = nextState;
            isFirstFrame = true;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        isGround = true;
    }
}
