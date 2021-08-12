using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStepBackward : MonoBehaviour
{
    public LayerMask groundLayer;
    private Rigidbody2D Character;
    private void Start()
    {
        Character = GetComponentInParent<Rigidbody2D>();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & groundLayer) != 0))
        {
            if(PlayerMovement.instance._isMoveable && PlayerManager.instance.CanWalkOnStairs &&!(PlayerManager.instance.input.GetKey(InputAction.Jump)||PlayerManager.state == PlayerManager.StateCode.Jump))
            {
                float input = PlayerMovement.instance.WalkOnStaris();
                if (input == 0)
                {
                    Character.velocity = new Vector2(0, 0.1965f);
                }
                //use 1st bit : 0b1
                PlayerManager.instance.ableToSprint &= 0b11111110;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //use 1st bit : 0b1
        PlayerManager.instance.ableToSprint |= 0b00000001;
    }
}
