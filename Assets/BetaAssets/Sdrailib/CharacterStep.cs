using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStep : MonoBehaviour
{
    public LayerMask groundLayer;
    public Rigidbody2D Character;
    private void Start()
    {
        Character = GetComponentInParent<Rigidbody2D>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Character.position = new Vector2(Character.position.x, Character.position.y + 0.1f);
        }
    }
}
