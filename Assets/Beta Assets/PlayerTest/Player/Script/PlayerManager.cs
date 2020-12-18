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

    public static bool isTalking = false;
    public static bool isFlying = false;
    public static bool isJumping = false;

    public static int state = 0;
    public static int hp;
    public static int damage;
}