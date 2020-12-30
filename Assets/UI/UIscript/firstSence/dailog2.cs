using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class dailog2 : MonoBehaviour
{
    public GameObject block;
    public Animation dai;
    public Text dai2;
    private bool talked = false;
    void Start()
    {
        dai2.text = " ";   
    }

    // Update is called once per frame
     void OnTriggerEnter2D(Collider2D coll)
     {
         Destroy(block,2);
         if(coll.tag == "Player" && !talked){
            dai2.text = "主角 : 前面過不去了，先到下面看看吧";
            dai["對話框"].time = 0;
            dai["對話框"].speed = 1;
            dai.Play("對話框");
         }
     }

     void OnTriggerExit2D(Collider2D coll)
     {
       if(coll.tag == "Player" && !talked){
            dai2.text = "";
            talked = true;
            dai["對話框"].time = dai["對話框"].length;
            dai["對話框"].speed = -1;
            dai.Play("對話框");
         }
     }
}
