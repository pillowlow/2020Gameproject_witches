using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class transform : MonoBehaviour
{
    public GameObject arr1,arr2,arr3,arr4;
    public Animation dai;
    public Text dai1;
    private bool talked = false;
    void Start()
    {
        dai1.text = " ";   
    }

    // Update is called once per frame
     void OnTriggerEnter2D(Collider2D coll)
     {
         
         if(coll.tag == "Player" && !talked){
            dai1.text = "主角 : !??";
            PlayerManager.sanityValue = 50;
            PlayerManager.isTalking = true;
            dai["對話框"].time = 0;
            dai["對話框"].speed = 1;
            dai.Play("對話框");
            Invoke( "next" , 3f);
         }
     }


    void next()
    {
        dai1.text = "按下P鍵變身";
        Invoke( "next1" , 3f);
    }
    void next1()
    {
        dai1.text = "主角 : 這樣就能去剛剛到不了的對岸了!!";
        Invoke( "next2" , 3f);
    }
    void next2()
    {
        
        dai1.text = "連續按下空白鍵以飛行";
        Invoke( "exitdai" , 3f);
    }

    void exitdai()
    {
        dai1.text = " ";
        PlayerManager.isTalking = false;
        talked = true;
        dai["對話框"].time = dai["對話框"].length;
        dai["對話框"].speed = -1;
        dai.Play("對話框");
        arr1.SetActive(true);
        arr2.SetActive(true);
        arr3.SetActive(true);
        arr4.SetActive(true);
    }
}