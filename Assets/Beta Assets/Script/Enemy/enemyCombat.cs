using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyCombat : MonoBehaviour
{
    private Animator animator;
    private AudioSource AudioSource;
    private enemyUnit EnemyUnit;

    public AudioClip deathSound;
    
    private int currentHealth;
    public bool destroy;
    public float destroyTime;
    void Start()
    {
        EnemyUnit = GetComponent<enemyUnit>();
        animator = GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();
        
        currentHealth = EnemyUnit.maxHealth;
        
        animator.SetBool("Grounded", true);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        animator.SetTrigger("Hurt");
        
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        animator.SetTrigger("Death");
        AudioSource.PlayOneShot(deathSound);
        GetComponent<enemyCombat>().enabled = false;
        gameObject.layer =11;
        if (destroy)
        {
            yield return new WaitForSeconds(destroyTime);
            Destroy(gameObject);
        }
    }
}
