using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rig2D;
    [SerializeField] ParticleSystem particle;
    [SerializeField] int hight = 200;
    bool used; //save
    void Start()
    {
        animator = GetComponent<Animator>();
        rig2D = transform.parent.GetComponent<Rigidbody2D>();
    }
    void OnTriggerEnter2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12 || used) { return; }
        PlayerMovement playerMovement = Player.gameObject.GetComponent<PlayerMovement>();
        if(playerMovement != null)
        {
            playerMovement.Killed();
        }
        animator.SetTrigger("Trigger");
        rig2D.AddForce(Vector2.up * hight);
        used = true;
    }
    public void TriggerParticle() //animation event
    {
        particle.Play(); 
    }
}
