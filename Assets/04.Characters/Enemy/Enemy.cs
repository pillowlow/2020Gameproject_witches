using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Info")]
    public int hp ;
    public int damage;
    private bool isDie = false;
    
    private StateCode state = StateCode.Idle;

    enum StateCode
    {
        Idle, Jumping, Moving, Die
    }
    

    private void OnCollisionEnter2D(Collision2D col)
    {
        if(state==StateCode.Die) return;
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerManager.TakeDamage(damage,transform);
        }
    }
    

    public void Damaged(int damageInput)
    {
        if (state != StateCode.Die)
        {
            hp = hp - damageInput;
            hp = (hp < 0) ? 0 : hp;
            if (hp == 0)
            {
                Die();
            }
        }
    }
    public void Damaged(int damageInput,Transform player)
    {
        if (state != StateCode.Die)
        {
            hp = hp - damageInput;
            hp = (hp < 0) ? 0 : hp;
            if (hp == 0)
            {
                Die();
            }
            Vector2 dir = (transform.position - player.position).normalized;
            GetComponent<Rigidbody2D>().AddForce(dir*100f);
        }
        
    }

    private void Die()
    {
        state = StateCode.Die;
        gameObject.transform.Rotate(Vector3.forward * -90);
    }

    public int GetHp()
    {
        return hp;
    }
}
