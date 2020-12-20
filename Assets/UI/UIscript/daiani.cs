using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class daiani : MonoBehaviour
{
    public Animation villhead,Villager,Villager_ani,Princess,player,dailog;
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
                switch(PlayerManager.talk_man)
                {
                    case 1:
                        villhead["村長對話"].time = 0;
                        villhead["村長對話"].speed = 1;
                        villhead.Play("村長對話");
                        break;
                    case 2:
                        Villager_ani.enabled = false;
                        Villager["理智值村民UI"].time = 0;
                        Villager["理智值村民UI"].speed = 1;
                        Villager.Play("理智值村民UI");
                        break;
                    case 3:
                        Princess["公主UI"].time = 0;
                        Princess["公主UI"].speed = 1;
                        Princess.Play("公主UI");
                        break;
                    case 4:

                        break;
                    case 5:
                        
                        break;
                    case 6:
                    
                        break;
                }
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
}
