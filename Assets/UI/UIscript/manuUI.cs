using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class manuUI : MonoBehaviour
{
    public GameObject title,title2,title3,start,setting,exit,particle;
    public bool picnum;
    public Text t_start,t_setting,t_exit;
    Color Imagecolor,Image2color,c_start,c_setting,c_exit;
    // Start is called before the first frame update
    void Start()
    {
        Imagecolor = new Color(1f,1f,1f,0);
        Image2color = new Color(1f,1f,1f,0);
        c_start = new Color(1f,1f,1f,0);
        c_setting = new Color(1f,1f,1f,0);
        c_exit = new Color(1f,1f,1f,0);
        picnum = true;
        InvokeRepeating("change_picture", 0.1f, 8.0f);

    }

    // Update is called once per frame
    void Update()
    {
        if(Imagecolor.a <= 1) Imagecolor.a += 0.01f;
        if(Image2color.a <= 1 && Imagecolor.a >= 0.5f) Image2color.a += 0.01f;

        title.GetComponent<Image>().color = Imagecolor;
        title2.GetComponent<Image>().color = Image2color;
        title3.GetComponent<Image>().color = Imagecolor;


        if(Imagecolor.a >= 0.5f && c_start.a <= 0.7f) c_start.a += 0.01f;
        if(c_start.a >= 0.5f && c_setting.a <= 0.7f) c_setting.a += 0.01f; 
        if(c_setting.a >= 0.5f && c_exit.a <= 0.7f) c_exit.a += 0.01f;

        t_start.color = c_start;
        t_setting.color = c_setting;
        t_exit.color = c_exit;
        start.GetComponent<Image>().color = c_start;
        setting.GetComponent<Image>().color = c_setting;
        exit.GetComponent<Image>().color = c_exit;

        if(Input.GetMouseButtonDown(0)){
            //Vector3 camera = Camera.main.WorldToScreenPoint(Camera..tanstorm.position);// 相機是世界的，世界到螢幕
            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            pos = Camera.main.ScreenToWorldPoint(pos) + new Vector3(1.8f,-2.15f,0);
            GameObject del = Instantiate(particle, pos, particle.transform.rotation);
            Destroy(del,2);
        }
    }

    void change_picture()
    {
        picnum = !picnum;
    }

    public void START()
    {
        SceneManager.LoadScene(1);
    }

    public void Quit (){

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
