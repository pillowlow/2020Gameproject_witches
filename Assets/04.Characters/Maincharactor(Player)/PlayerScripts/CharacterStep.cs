using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStep : MonoBehaviour
{
    public LayerMask groundLayer;
    private Rigidbody2D Character;
    public float StepHeight = 0.2f;
    public PhysicsMaterial2D material;
    private void Start()
    {
        Character = GetComponentInParent<Rigidbody2D>();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & groundLayer) != 0))
        {
            if(PlayerManager.instance.input.GetHorizonInput() != 0)
            {
                Vector2 dir = new Vector2((PlayerMovement.instance.orient ? StepHeight : -StepHeight), StepHeight) * Time.deltaTime;
                Character.MovePosition(Character.position + dir);
                material.friction = 2;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        material.friction = 0.1f;
    }
}
