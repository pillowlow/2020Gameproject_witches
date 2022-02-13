using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStep : MonoBehaviour
{
    public LayerMask groundLayer;
    private Rigidbody2D Character;
    public float StepHeight = 0.2f;
    private void Start()
    {
        Character = GetComponentInParent<Rigidbody2D>();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & groundLayer) != 0))
        {
            if(PlayerMovement.instance._isMoveable && PlayerManager.instance.CanWalkOnStairs && !(PlayerManager.instance.input.GetKeyDown(InputAction.Jump) || PlayerManager.state == PlayerManager.StateCode.Jump))
            {
                float input = PlayerMovement.instance.WalkOnStaris();
                if (input != 0)
                {
                    RaycastHit2D A = Physics2D.Raycast(transform.position,Vector2.down, Mathf.Infinity,groundLayer);
                    RaycastHit2D B = Physics2D.Raycast(PlayerManager.instance.player.transform.position, Vector2.down, Mathf.Infinity, groundLayer);
                    if (A.collider != null && B.collider != null)
                    {
                        float slope = Mathf.Abs((A.point.y - B.point.y) / (A.point.x - B.point.x));
                        float a = (Mathf.PI * 0.47f) / (slope + 1);//(0,inf) -> (1,0)
                        Vector2 dir = new Vector2(Mathf.Sin(a) * input, Mathf.Cos(a)) * StepHeight * Time.deltaTime;
                        Character.MovePosition(Character.position + dir);
                    }
                }
                Character.velocity = new Vector2(0, 0.1965f);
                //use 2nd bit : 0b10
                PlayerManager.instance.ableToSprint &= 0b11111101;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //use 2nd bit : 0b10
        PlayerManager.instance.ableToSprint |= 0b00000010;
    }
}
