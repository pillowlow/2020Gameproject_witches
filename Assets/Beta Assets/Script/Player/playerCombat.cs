using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class playerCombat : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private playerController PlayerController;
    private playerUnit Unit;
    private AudioSource audioSource;

    public AudioClip getHurt;
    public AudioClip death;
    public AudioClip hitenemy;
    
    public Transform attackPoint;
    public LayerMask enemyLayers;

    public float attackRange = 0.5f;

    private float nextAttackTime = 0f;
    public Transform feetPos;

    private int currentHealth;

    public bool isInvincible = false;
    private bool isReborn = false;

    public Vector2 collisionSize;

    public Image[] hearts;
    public Sprite Heart;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        PlayerController = GetComponent<playerController>();
        Unit = GetComponent<playerUnit>();
        audioSource = GetComponent<AudioSource>();
        
        //set currentHealth
        currentHealth = Unit.maxHealth;
    }
    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                Attack();
                nextAttackTime = Time.time + 1f / Unit.attackSpeed;
            }
        }
        //非Invincible下會受到Collision Damage
        if (!isInvincible)
        {
            CollisionDamage();
        }
        HealthUI();
    }
    void Attack()
    {
        animator.SetTrigger("Attack");
        if (PlayerController.OnGround)
        {
            ////在attackPoint創建一個Circle碰撞器[]
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
            foreach (Collider2D enemy in hitEnemies)
            {
                audioSource.PlayOneShot(hitenemy);
                enemy.GetComponent<enemyCombat>().TakeDamage(Unit.attackDamage);
            }
        }
        else
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(feetPos.position, attackRange, enemyLayers);
            foreach (Collider2D enemy in hitEnemies)
            {
                audioSource.PlayOneShot(hitenemy);
                enemy.GetComponent<enemyCombat>().TakeDamage(Unit.attackDamage);
                rb.velocity = new Vector2(rb.velocity.x, Unit.jumpAttackForce);
            }
        }
    }
    private void CollisionDamage()
    {
        Collider2D[] collision = Physics2D.OverlapBoxAll(new Vector2(gameObject.transform.position.x,gameObject.transform.position.y+1.5f), collisionSize,0.0f, enemyLayers);
        foreach (Collider2D enemy in collision)
        {
            TakeDamage(enemy.GetComponent<enemyUnit>().atk);
            //rb.velocity=new Vector2(0,0);
            //if (gameObject.transform.position.x < enemy.transform.position.x)
            //{
            //    rb.AddForce(new Vector2(-Unit.Knockback.x, Unit.Knockback.y));
            //}
            //else
            //{
            //    rb.AddForce(new Vector2(Unit.Knockback.x, Unit.Knockback.y));
            //}
        }
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("Hurt");
        audioSource.PlayOneShot(getHurt);
        
        if (currentHealth <= 0)
        {
            die();
        }
        else
        {
            StartCoroutine(Invincible(Unit.invincibleTime));
        }
    }
    private IEnumerator Invincible(float Time)
    {
        isInvincible = true;
        yield return new WaitForSeconds(Time);
        isInvincible = false;
    }
    private void die()
    {
        animator.SetTrigger("Death");
        audioSource.PlayOneShot(death);
        rb.velocity=new Vector2(0,0);
        Invincible(3.8f);
        gameObject.tag = "";
        GetComponent<playerCombat>().enabled = false;
        GetComponent<playerController>().enabled = false;
        StartCoroutine(Reborn());
    }

    private IEnumerator Reborn()
    {
        print(Unit.rebornScene);
        yield return new WaitForSeconds(3);
        isReborn = true;
        SceneManager.LoadScene(Unit.rebornScene);
        transform.position=GameObject.Find("rebornPoint").transform.position;
        currentHealth = Unit.maxHealth;
        animator.SetTrigger("Recover");
        yield return new WaitForSeconds(0.8f);
        gameObject.tag = "Player";
        GetComponent<playerCombat>().enabled = true;
        GetComponent<playerController>().enabled = true;
        
    }
    private void HealthUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < Unit.maxHealth)
            {
                hearts[i].sprite = Heart;
            }
            else
            {
                hearts[i].sprite = null;
            }
            if (i < currentHealth)
            {
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled=false;
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        Gizmos.DrawWireSphere(feetPos.position, attackRange);
        Gizmos.DrawWireCube(new Vector2(gameObject.transform.position.x,gameObject.transform.position.y+1.5f), collisionSize);
    }
}
