using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using TMPro;
public class daisys : MonoBehaviour
{
    [Header("UI組件")]
    public Text text_labe;
    [Header("文本文件")]
    public TextAsset textfile;
    private int index,playertalk=0;
    public float textspeed;
    bool textfinish;
    public Animation  villhead,player,dailog;
    public Image m_villhead,m_player;
    List<string> textList = new List<string>();
    // Start is called before the first frame update
    void Awake()
    {
        Gettext(textfile);
        textspeed = 0.02f;
    }

    private void OnEnable()
    {
        //StartCoroutine(SetTextUI());
        textfinish = true;
        text_labe.text = " ";
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerManager.talk_man == 1){
            if(Input.GetKeyDown(KeyCode.Tab) &&　index == textList.Count)
            {
                index = 0;
                playertalk = 0;
                text_labe.text = textList[index];
                villhead["村長對話"].time = villhead["村長對話"].length;
                villhead["村長對話"].speed = -1;
                villhead.Play("村長對話");
                player["主角UI"].time = player["主角UI"].length;
                player["主角UI"].speed = -1;
                player.Play("主角UI");
                dailog["對話框"].time = dailog["對話框"].length;
                dailog["對話框"].speed = -1;
                dailog.Play("對話框");
            }else if(Input.GetKeyDown(KeyCode.Tab) && textfinish && PlayerManager.talkable)
            {
                playertalk++;
                StartCoroutine(SetTextUI());
            }

            if(playertalk == 3 ||playertalk == 7 || playertalk == 9 || playertalk == 11 || playertalk == 15)
            {
                m_villhead.color = new Color(0.3f,0.3f,0.3f,1);
                m_player.color = new Color(1,1,1,1);
            }else
            {
                m_player.color = new Color(0.3f,0.3f,0.3f,1);
                m_villhead.color = new Color(1,1,1,1);
            }
        }
    }

    void Gettext(TextAsset file)
    {
        textList.Clear();
        index = 0;

        var linedata = file.text.Split('\n');

        foreach (var line in linedata)
        {
            textList.Add(line);
        }
    }

    IEnumerator SetTextUI()
    {
        textfinish = false;
        text_labe.text = " ";
        for(int i=0;i<textList[index].Length;i++)
        {
            text_labe.text += textList[index][i];

            yield return new WaitForSeconds(textspeed);
        }
        textfinish = true;
        index++;
    }
}
