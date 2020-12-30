using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class dailog1 : MonoBehaviour
{
    public GameObject block;
    public Animation dai;
    public GameObject text1;
    private bool talked = false;
    // Start is called before the first frame update
     void OnTriggerEnter2D(Collider2D coll)
     {
         if(coll.tag == "Player" && !talked){
            Destroy(block,2);
            dai["對話框"].time = 0;
            dai["對話框"].speed = 1;
            dai.Play("對話框");
         }
     }

    void OnTriggerExit2D(Collider2D coll)
     {
         if(coll.tag == "Player" && !talked){
            talked = true;
            dai["對話框"].time = dai["對話框"].length;
            dai["對話框"].speed = -1;
            dai.Play("對話框");
            text1.SetActive(false);
         }
     }
}
