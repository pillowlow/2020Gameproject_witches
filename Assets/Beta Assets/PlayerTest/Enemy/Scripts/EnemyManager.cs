using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Info")]
    public int hp;
    public int damage;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damaged(int damageInput)
    {
        hp = hp - damageInput;
        hp = (hp < 0) ? 0 : hp;
        Debug.Log(hp);
    }
}
