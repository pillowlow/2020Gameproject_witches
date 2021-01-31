using System;
using UnityEngine;
using UnityEngine.UI;

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
    public String PopupText = "按F互動";
    private Text popup;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            //When Player enter then show floating text on the object
            popup = TextUI.transform.Find("PopUpText").GetComponent<Text>();
            Vector2 pos = gameObject.transform.position;
            TextUI.transform.position = new Vector2(pos.x, pos.y + offset);
            popup.text = PopupText;
            active = true;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            Vector2 pos = gameObject.transform.position;
            TextUI.transform.position = new Vector2(pos.x, pos.y + offset);
            popup.text = PopupText;
            if (!active)
            {
                popup.text = "";
            }
            //If Player press F then call DoAction
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
            //When Player exit then disable floating text
            active = false;
            popup.text = "";
        }
    }
    
    
    public void DoAction()
    {   
        //Different Actions
        //Maybe I will make an event factory later
        switch (ObjectAction)
            {
                case Actions.Story:
                    TextUI.GetComponent<TextUIScript>().LoadText(TextPath,this);
                    break;
                case Actions.Item:
                    break;
                case Actions.Event:
                    break;
            }

    }

}
