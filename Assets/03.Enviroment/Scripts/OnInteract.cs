using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static objectTextUI;

public class OnInteract : MonoBehaviour
{
    
    
    public enum Actions
    {
        Story,Item,Event        
    }
    
    public GameObject TextUI; 
    public bool active = false;
    public Actions ObjectAction;
    public String TextPath;
    public float offset=4.0f;
    private Text popup;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            popup = TextUI.transform.Find("PopUpText").GetComponent<Text>();
            Vector2 pos = gameObject.transform.position;
            TextUI.transform.position = new Vector2(pos.x, pos.y + offset);
            popup.text = "按F互動";
            active = true;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            Vector2 pos = gameObject.transform.position;
            TextUI.transform.position = new Vector2(pos.x, pos.y + offset);
            popup.text = "按F互動";
            if (!active)
            {
                popup.text = "";
            }
            if (active && Input.GetKeyDown(KeyCode.F) )
            {
                active = false;
                DoAction();
            }
        }
        
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            active = false;
            popup.text = "";
        }
    }
    
    public void DoAction()
    {
        switch (ObjectAction)
            {
                case Actions.Story:
                    TextUI.GetComponent<objectTextUI>().LoadText(TextPath,this);
                    break;
                case Actions.Item:
                    break;
                case Actions.Event:
                    break;
            }

    }

}
