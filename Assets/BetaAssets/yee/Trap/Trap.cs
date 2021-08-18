using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rigidbody2D;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] int hight;
    bool used; //save
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody2D = transform.parent.GetComponent<Rigidbody2D>();
    }
    void OnTriggerEnter2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12 || used) { return; }
        animator.SetTrigger("Trigger");
        rigidbody2D.AddForce(Vector2.up * hight);
        used = true;
    }
    public void TriggerParticle() //animation event
    {
        particleSystem.Play(); 
    }
}
