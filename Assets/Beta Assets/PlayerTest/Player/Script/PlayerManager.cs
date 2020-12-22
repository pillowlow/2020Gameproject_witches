using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton
    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    public GameObject player;
    public LayerMask enemyLayer;

    public static bool isTalking = false;
    public static bool talkable = false;
    public static bool isFlying = false;
    public static bool isJumping = false;
    public static bool moveable = true;
    public static bool onGround = false;
    public static int talk_man;

    public static ModeCode mode = ModeCode.normal;
    public static StateCode state = StateCode.idel;
    public static int hp = 100;
    public static int damage = 10;

    public enum StateCode {
        idel, moving, jumping, falling, flying, takingHit, attack1, 
        attack1_connection , attack2 , attack2_connection
    };

    public enum ModeCode{
        normal, transform
    };

    public static class Exp
    {
        static int lv = 0;
        static int value = 0;

        public static int GetValue()
        {
            return value;
        }
        public static int GetLv()
        {
            return lv;
        }
        public static void AssignValue(int val)
        {
            value = val;
        }
        public static void AssignLv(int val)
        {
            lv = val;
        }
    }

    public static class Damage
    {
        static int lvDamage;
        static int equipDamage;

        public static int GetDamage()
        {
            return lvDamage + equipDamage;
        }
        public static int GetLvDamage()
        {
            return lvDamage;
        }
        public static int GetEquipDamage()
        {
            return equipDamage;
        }
        public static void AssignLvDamage(int damage)
        {
            lvDamage = damage;
        }
        public static void AssignEquipDamage(int damage)
        {
            equipDamage = damage;
        }
    }
}

