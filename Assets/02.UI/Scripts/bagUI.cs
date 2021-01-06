using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bagUI : MonoBehaviour
{
    public Animation ani;
    public string aniname = "change";
    public void Change()
    {
        if (!ani.isPlaying)
        {
            ani[aniname].time = 0;
            ani[aniname].speed = 1;
            ani.Play(aniname);
        }
    }

    public void change_left()
    {   
        if (!ani.isPlaying)
        {
            ani[aniname].time = ani[aniname].length;
            ani[aniname].speed = -1;
            ani.Play(aniname);
        }
    } 
}
