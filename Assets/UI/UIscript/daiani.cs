using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class daiani : MonoBehaviour
{
    public Animation villhead,player,dailog;
    // Start is called before the first frame update
    void Start()
    {
        PlayerManager.isTalking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerManager.talkable)
        {
            if(Input.GetKeyDown(KeyCode.Tab) && !PlayerManager.isTalking)
            {
                villhead["村長對話"].time = 0;
                villhead["村長對話"].speed = 1;
                villhead.Play("村長對話");
                player["主角UI"].time = 0;
                player["主角UI"].speed = 1;
                player.Play("主角UI");
                dailog["對話框"].time = 0;
                dailog["對話框"].speed = 1;
                dailog.Play("對話框");
                PlayerManager.isTalking = true;
            }
        }    
    }

    void OnTriggerExit2D(Collider2D coll)
    {
    }

}
