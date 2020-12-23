using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class playerUnit : MonoBehaviour
{
    //裝參數用,方便調整
    public int maxHealth;//
    public int attackDamage;
    public float attackSpeed;//per second
    public float jumpAttackForce;//下斬上升幅度
    public float jumpForce;
    public float jumpTime;//跳躍最長時間(second)
    public float minHeight;//跳躍最低高度
    public float speed;
    public float invincibleTime;
    public float RecoverTime;
    public Vector2 Knockback;
    public int moneny;
    public string rebornScene;
    public string nextScene;
    
}
