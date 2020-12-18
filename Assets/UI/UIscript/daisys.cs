using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class daisys : MonoBehaviour
{
    [Header("UI組件")]
    public Text text_labe;
    [Header("文本文件")]
    public TextAsset textfile;
    public int index;
    public Animation left,right,down;
    public SpriteRenderer m_left,m_right;
    List<string> textList = new List<string>();
    // Start is called before the first frame update
    void Awake()
    {
        Gettext(textfile);
    }

    private void OnEnable()
    {
        text_labe.text = " ";

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F) &&　index == textList.Count)
        {
            index = 0;
            text_labe.text = textList[index];
            left["zoomleft"].time = left["zoomleft"].length;
            left["zoomleft"].speed = -1;
            left.Play("zoomleft");
            right["zoomright"].time = right["zoomright"].length;
            right["zoomright"].speed = -1;
            right.Play("zoomright");
            down["zoomup"].time = down["zoomup"].length;
            down["zoomup"].speed = -1;
            down.Play("zoomup");
            PlayerManager.isTalking = false;
            return;
        }
        if(Input.GetKeyDown(KeyCode.F))
        {
            text_labe.text = textList[index];
            index++;
        }

        if((index%2) == 0)
        {
            m_left.color = new Color(0.3f,0.3f,0.3f,1);
            m_right.color = new Color(1,1,1,1);
        }else if((index%2) == 1)
        {
            m_right.color = new Color(0.3f,0.3f,0.3f,1);
            m_left.color = new Color(1,1,1,1);
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
}
