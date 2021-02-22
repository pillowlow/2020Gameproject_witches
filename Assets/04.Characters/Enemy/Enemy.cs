using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Info")]
    private int hp ;
    private int damage;
    private bool isDie = false;
    
    private StateCode state = StateCode.Idle;

    enum StateCode
    {
        Idle, Jumping, Moving, Die
    }
    

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
