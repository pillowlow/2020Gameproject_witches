using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Twovillagerdai : MonoBehaviour
{
[Header("UI組件")]
    public Text text_labe;
    [Header("文本文件")]
    public TextAsset textfile;
    private int index,playertalk=0;
    public float textspeed;
    bool textfinish;
    public Animation  NPC,NPC2,player,dailog;
    public Image m_NPC,m_player;
    List<string> textList = new List<string>();

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

    void Update()
    {
        if(PlayerManager.talk_man == 4)
        {
            if((Input.GetKeyDown(KeyCode.Tab) &&　index == textList.Count) || (PlayerManager.isTalking && Input.GetKeyDown(KeyCode.Escape)))
            {
                index = 0;
                playertalk = 0;
                text_labe.text = " ";
                NPC["村民3動畫UI"].time = NPC["村民3動畫UI"].length;
                NPC["村民3動畫UI"].speed = -1;
                NPC.Play("村民3動畫UI");
                NPC2["村民2動畫UI"].time = NPC2["村民2動畫UI"].length;
                NPC2["村民2動畫UI"].speed = -1;
                NPC2.Play("村民2動畫UI");
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

            if(playertalk == 2 ||playertalk == 4 ||playertalk == 6 ||playertalk == 7 || playertalk == 9)
            {
                m_NPC.color = new Color(0.3f,0.3f,0.3f,1);
                m_player.color = new Color(1,1,1,1);
            }else
            {
                m_player.color = new Color(0.3f,0.3f,0.3f,1);
                m_NPC.color = new Color(1,1,1,1);
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
