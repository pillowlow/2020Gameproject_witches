using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BOSSUI : MonoBehaviour
{
    public GameObject Boss;
    private int bossHP;
    public Image playerHP,playerSan,BossHP;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bossHP = Boss.GetComponent<EnemyManager>().GetHp();
        playerHP.fillAmount = PlayerManager.hp/100.0f;
        BossHP.fillAmount = bossHP/100.0f;
    }
}
