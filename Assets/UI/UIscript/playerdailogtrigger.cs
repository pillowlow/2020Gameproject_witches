using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerdailogtrigger : MonoBehaviour
{
    public Animation mark;
    void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.tag == "village head")
        {
            PlayerManager.talkable = true;
            mark["Exclamation mark"].time = 0;
            mark["Exclamation mark"].speed = 1;
            mark.Play("Exclamation mark");
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        PlayerManager.talkable = false;
        if(coll.tag == "village head")
        {
            mark["Exclamation mark"].time = mark["Exclamation mark"].length;
            mark["Exclamation mark"].speed = -1;
            mark.Play("Exclamation mark");
        }
    }
}
