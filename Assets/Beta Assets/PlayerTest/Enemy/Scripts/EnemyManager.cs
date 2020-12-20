using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Info")]
    public int hp;
    public int damage;
    public int exp;

    public void Damaged(int damageInput)
    {
        hp = hp - damageInput;
        hp = (hp < 0) ? 0 : hp;
        Debug.Log(hp);
    }
}
