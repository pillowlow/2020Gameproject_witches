using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class deadUI : MonoBehaviour
{
    public GameObject dead;
    public Text dead_mess,restart;
    public Button dead_bott;
    Color dead_color,icon_color,deadicon;
    // Start is called before the first frame update
    void Start()
    {
        //dead_mess.text = "敗北";
        icon_color = new Color(1,1,1,0);
        deadicon = new Color(1,0,0,0);
        dead_color = new Color(0f,0f,0f,0);
    }

    // Update is called once per frame
    void Update()
    {
        if(dead_color.a < 1) dead_color.a += 0.05f;
        if(icon_color.a < 1) icon_color.a += 0.05f;
        if(deadicon.a < 1) deadicon.a += 0.05f;
        dead_mess.color = deadicon;
        dead_bott.image.color = icon_color;
        restart.color = dead_color;
        dead.GetComponent<Image>().color = dead_color;
    }

    public void restartfun(int sen)
    {
        SceneManager.LoadScene(sen);
    }
}
