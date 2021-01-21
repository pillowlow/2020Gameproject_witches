using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Info")]
    public int hp ;
    public int damage;
    public int exp;
    public bool isDie = false;
    public StateCode state = StateCode.idle;

    public enum StateCode {idle, jumping, moving, die};

    public void Damaged(int damageInput)
    {
        hp = hp - damageInput;
        hp = (hp < 0) ? 0 : hp;


    }

    public int GetHp()
    {
        return hp;
    }
}
