using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowPlayerExp : MonoBehaviour
{
    public string lable;
    public enum showIn {Lv,Exp};
    public showIn type;

    Text text;
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        int lv = PlayerManager.Exp.GetLv();

        if (type == showIn.Lv)
            text.text = lable + " " + lv.ToString();
        else if (type == showIn.Exp)
        {
            text.text = lable + " " + PlayerManager.Exp.GetValue().ToString() + "/" + ExpSystem.GetEachLvValue(lv).ToString();
        }
    }
}
