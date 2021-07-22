using System;
using DragonBones;
using UnityEngine;
using Transform = UnityEngine.Transform;

public class Enemy : MonoBehaviour
{
    [Header("Info")]
    public int hp ;
    public int Damage;

    protected StateCode state = StateCode.Idle;

    protected enum StateCode
    {
        Idle, Jumping, Moving, Die
    }
    

    private void OnCollisionStay2D(Collision2D col)
    {
        if(state==StateCode.Die) return;
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerManager.TakeDamage(Damage,transform);
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

    protected virtual void Die()
    {
        state = StateCode.Die;
        Physics2D.IgnoreCollision(PlayerManager.instance.player.GetComponent<Collider2D>(),GetComponent<Collider2D>());
        gameObject.transform.Rotate(Vector3.forward * -90);
    }
}
