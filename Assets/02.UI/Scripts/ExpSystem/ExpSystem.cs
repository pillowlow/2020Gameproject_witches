using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpSystem : MonoBehaviour
{
    static int[] eachLvValue = new int[10]
    {40, 80, 120, 160, 200, 240, 280, 320, 360, 400};

    GameObject player;
    void Start()
    {
        player = PlayerManager.instance.player;
    }

    void Update()
    {
        
    }

    public static void UpdateExp(int exp)
    {
        int lv = PlayerManager.Exp.GetLv();
        int value = PlayerManager.Exp.GetValue() + exp;

        //LevelUp
        if (value >= eachLvValue[lv]) 
        {
            value = eachLvValue[lv] - value;
            lv = lv + 1;
            PlayerManager.Exp.AssignLv(lv);
            LevelUp();
        }

        PlayerManager.Exp.AssignValue(value);
    }

    public static void LevelUp()
    {

    }

    public static int GetEachLvValue(int lv)
    {
        return eachLvValue[lv];
    }
}
