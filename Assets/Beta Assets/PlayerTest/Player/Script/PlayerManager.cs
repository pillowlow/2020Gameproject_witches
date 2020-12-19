using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
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

    public static int mode = 0;
    public static int state = StateCode.idel;
    public static int hp = 100;
    public static int damage = 10;

    public static class StateCode
    {
        public static int idel = 0;
        public static int moving = 1;
        public static int jumping = 2;
        public static int falling = 3;
        public static int attack1 = 4;
        public static int attack1_connection = 5;
        public static int attack2 = 6;
        public static int attack2_connection = 7;
    }
}

