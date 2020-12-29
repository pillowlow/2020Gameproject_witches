using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_TakeDamage : MonoBehaviour
{
    Rigidbody2D rig;
    Animator anim;

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public static void TakeDamage(int damage)
    {
        int hp = PlayerManager.hp;

        if(PlayerManager.state != PlayerManager.StateCode.takingHit)
        {
            hp = (hp - damage > 0) ? hp - damage : 0;
            PlayerManager.state = PlayerManager.StateCode.takingHit;
        }

        PlayerManager.hp = hp;
    }

    public void TakeHitEnd()
    {
        PlayerManager.state = PlayerManager.StateCode.idel;
    }
}
