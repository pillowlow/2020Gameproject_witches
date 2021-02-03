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
             Dialog.Instance.ShowTextArea("test");
         }
     }

    void OnTriggerExit2D(Collider2D coll)
     {
         if(coll.tag == "Player"){
            Destroy(gameObject);
            Dialog.Instance.HideTextArea();
         }
     }
}
