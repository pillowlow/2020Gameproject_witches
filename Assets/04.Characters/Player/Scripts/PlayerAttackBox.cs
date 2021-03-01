using System;
using UnityEngine;

public class PlayerAttackBox : MonoBehaviour
{
    private int AttackValue;
    private void Start()
    {
        AttackValue = PlayerManager.Damage.GetDamage();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            col.GetComponent<Enemy>().Damaged(AttackValue,transform);
        }
    }
}