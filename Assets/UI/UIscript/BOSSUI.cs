using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class BOSSUI : MonoBehaviour
{
    public GameObject Boss,dead_mess,win;
    private float bossHP;
    public Text t1,t2;
    public Image playerHP,playerSan,BossHP;
    Color textcolor;
    // Start is called before the first frame update
    void Start()
    {
        PlayerManager.hp = 100;
        PlayerManager.sanityValue = 100;
        dead_mess.SetActive(false);
        win.SetActive(false);
        textcolor = new Color(0,0,0,0);   
    }

    // Update is called once per frame
    void Update()
    {
        if(textcolor.a < 1) textcolor.a += 0.01f;
        t1.color = textcolor;
        t2.color = textcolor;
        if(textcolor.a >= 0.9f){
            Destroy(t1,3);
            Destroy(t2,3);
        }
        if(PlayerManager.hp == 0)
            dead_mess.SetActive(true);
        bossHP = Boss.GetComponent<EnemyManager>().GetHp();
        playerHP.fillAmount = PlayerManager.hp/100.0f;
        playerSan.fillAmount = PlayerManager.sanityValue/100.0f;
        BossHP.fillAmount = bossHP/100.0f;

        if(Boss.GetComponent<EnemyManager>().GetHp() == 0){
            win.SetActive(true);
        }
    }

    public void back()
    {
        SceneManager.LoadScene(0);
    }
}
