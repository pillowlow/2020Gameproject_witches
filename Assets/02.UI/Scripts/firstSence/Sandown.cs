using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Sandown : MonoBehaviour
{
    public GameObject block,block1;
    public Animation dai;
    // Start is called before the first frame update    public Animation dai;
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
            // dai1.text = "主角 : 這...!?這是!?怎麼頭昏昏的?得趕快離開這攤水才行!!";
            // Destroy(block,2);
            // Destroy(block1,2);
            // dai["對話框"].time = 0;
            // dai["對話框"].speed = 1;
            // dai.Play("對話框");
         }
     }

     void OnTriggerExit2D(Collider2D coll)
     {
       if(coll.tag == "Player" && !talked){
            // dai1.text = "主角 : 看來黑水會造成理智值降低，以後得小心點了!";
            // talked = true;
            // Invoke( "next" , 3f);
         }
     }

    // void next()
    // {
    //     dai1.text = "主角 : 上面平台那好像也有東西，不知道是什麼?";
    //     Invoke( "show" , 3f);
    // }
    //  void show()
    //  {
    //     dai1.text = "";
    //     dai["對話框"].time = dai["對話框"].length;
    //     dai["對話框"].speed = -1;
    //     dai.Play("對話框");
    //  }
}
