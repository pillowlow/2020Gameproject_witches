using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float Speed;

    EnemyManager myInfo;
    
    void Start()
    {
        myInfo = GetComponent<EnemyManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Die();
    }


    void Die()
    {
        if(myInfo.hp <= 0)
        {
            ExpSystem.UpdateExp(myInfo.exp);
            Destroy(this.gameObject);
        }
    }
}
