using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class manuUI : MonoBehaviour
{
    public GameObject title,title2,title3,left1,left2,start,setting,exit;
    public bool picnum;
    public Text t_start,t_setting,t_exit;
    Color Imagecolor,c_left1,c_left2,c_start,c_setting,c_exit;
    // Start is called before the first frame update
    void Start()
    {
        Imagecolor = new Color(1f,1f,1f,0);
        c_left1 = new Color(1f,1f,1f,1);
        c_left2 = new Color(1f,1f,1f,0);
        c_start = new Color(1f,1f,1f,0);
        c_setting = new Color(1f,1f,1f,0);
        c_exit = new Color(1f,1f,1f,0);
        picnum = true;
        InvokeRepeating("change_picture", 0.1f, 8.0f);

    }

    // Update is called once per frame
    void Update()
    {
        if(Imagecolor.a == 1) Imagecolor.a = 1;
        else Imagecolor.a += 0.001f;

        title.GetComponent<Image>().color = Imagecolor;
        title2.GetComponent<Image>().color = Imagecolor;
        title3.GetComponent<Image>().color = Imagecolor;

        if(picnum == true){
            if(c_left1.a <= 1f)  c_left1.a += 0.001f;
            if(c_left2.a >= 0) c_left2.a -= 0.005f;
        }else if(picnum == false){
            if(c_left2.a <= 1f) c_left2.a += 0.001f;
            if(c_left1.a >= 0)c_left1.a -= 0.005f;
        }

        left1.GetComponent<Image>().color = c_left1;
        left2.GetComponent<Image>().color = c_left2;

        if(Imagecolor.a >= 0.5f && c_start.a <= 0.7f) c_start.a += 0.001f;
        if(c_start.a >= 0.5f && c_setting.a <= 0.7f) c_setting.a += 0.001f; 
        if(c_setting.a >= 0.5f && c_exit.a <= 0.7f) c_exit.a += 0.001f;

        t_start.color = c_start;
        t_setting.color = c_setting;
        t_exit.color = c_exit;
        start.GetComponent<Image>().color = c_start;
        setting.GetComponent<Image>().color = c_setting;
        exit.GetComponent<Image>().color = c_exit;
    }

    void change_picture()
    {
        picnum = !picnum;
    }

    public void START()
    {
        SceneManager.LoadScene(3);
    }
}
