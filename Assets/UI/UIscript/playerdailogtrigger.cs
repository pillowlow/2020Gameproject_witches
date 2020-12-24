using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerdailogtrigger : MonoBehaviour
{
    public Animation mark;
    void OnTriggerEnter2D(Collider2D coll)
    {
        switch(this.name)
        {
            case "村長":
                PlayerManager.talk_man = 1;
                break;
            case "理智值村民":
                PlayerManager.talk_man = 2;
                break;
            case "公主":
                PlayerManager.talk_man = 3;
                break;
            case "村民2":
                PlayerManager.talk_man = 4;
                break;
            case "小女孩":
                PlayerManager.talk_man = 5;
                break;
        }
        if(coll.tag == "Player")
        {
            PlayerManager.talkable = true;
            mark["Exclamation mark"].time = 0;
            mark["Exclamation mark"].speed = 1;
            mark.Play("Exclamation mark");
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        PlayerManager.isTalking = false;
        PlayerManager.talkable = false;
        if(coll.tag == "Player")
        {
            mark["Exclamation mark"].time = mark["Exclamation mark"].length;
            mark["Exclamation mark"].speed = -1;
            mark.Play("Exclamation mark");
        }
    }
}
