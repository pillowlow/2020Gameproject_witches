using System;
using UnityEngine;
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
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            active = true;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
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
