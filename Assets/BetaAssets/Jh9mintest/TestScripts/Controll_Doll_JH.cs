using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnyPortrait;

public class Controll_Doll_JH : MonoBehaviour
{
    public apPortrait portrait;
    public Rigidbody2D rg2D;

    private enum State
    {
        Idle,
        Walk,
        Jump,
        Run,

    }
    private State state = State.Idle;

    private bool isFirstFrame = false;
    private bool isGround = true;
    private bool isHandle = false;

    // Start is called before the first frame update
    void Start()
    {
        state = State.Idle;
        isFirstFrame = true;
        isGround = true;
        isHandle = false;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.Idle:
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
            if(!isHandle)
                portrait.CrossFade("Idle", 0.2f);
            else
                portrait.CrossFade("Handle", 0.2f);
            isFirstFrame = false;
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            if (!isHandle)
            {
                isHandle = true;
                portrait.CrossFade("Take", 0.1f);
            }
            else
            {
                isHandle = false;
                portrait.CrossFade("Put", 0.1f);
            }
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            ChangeState(State.Walk);
        }
        else if (Input.GetKey(KeyCode.W) && isGround == true)
        {
            ChangeState(State.Jump);
        }
    }

    private void UpdateWalk()
    {
        if (isFirstFrame)
        {
            if(!isHandle)
                portrait.CrossFade("Walk", 0.3f);
            else
                portrait.CrossFade("HandleWalk", 0.3f);
            isFirstFrame = false;
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            if(!isHandle)
            {
                isHandle = true;
                portrait.CrossFade("Take", 0.1f);
            }
            else
            {
                isHandle = false;
                portrait.CrossFade("Put", 0.1f);
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetKey(KeyCode.LeftShift)&&!isHandle)
                ChangeState(State.Run);
            if (Input.GetKey(KeyCode.W) && isGround&&!isHandle)
                ChangeState(State.Jump);
            transform.Translate(new Vector3(-2f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.LeftShift)&&!isHandle)
                ChangeState(State.Run);
            if (Input.GetKey(KeyCode.W) && isGround&&!isHandle)
                ChangeState(State.Jump);
            transform.Translate(new Vector3(2f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
        }
        else
        {
            ChangeState(State.Idle);
        }

    }

    private void UpdateRun()
    {
        if (isFirstFrame)
        {
            portrait.CrossFade("Run", 0.2f);
            isFirstFrame = false;
        }
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.W) && isGround)
                ChangeState(State.Jump);
            transform.Translate(new Vector3(-4.5f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.W) && isGround)
                ChangeState(State.Jump);
            transform.Translate(new Vector3(4.5f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
        }
        else
        {
            if (!Input.GetKey(KeyCode.LeftShift))
                ChangeState(State.Walk);
            else
                ChangeState(State.Idle);
        }

    }

    private void UpdateJump()
    {
        if (isFirstFrame)
        {
            isGround = false;
            portrait.CrossFade("Jump", 0.1f);
            rg2D.AddForce(new Vector2(0, 50), ForceMode2D.Impulse);
            portrait.CrossFade("Fall", 0.1f);
            isFirstFrame = false;
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.Translate(new Vector3(-4.5f * Time.deltaTime, 0, 0));
            }
            else
                transform.Translate(new Vector3(-2f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else if (Input.GetKey(KeyCode.D))
        {

            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.Translate(new Vector3(4.5f * Time.deltaTime, 0, 0));
            }
            else
                transform.Translate(new Vector3(2f * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
        }


        if ((rg2D.velocity.y <= 0) && (isGround))
        {
            portrait.CrossFade("Land", 0.1f);
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    ChangeState(State.Run);
                else
                    ChangeState(State.Walk);
            }
            else
            {
                ChangeState(State.Idle);
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
