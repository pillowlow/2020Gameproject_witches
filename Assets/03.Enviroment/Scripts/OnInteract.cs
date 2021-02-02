using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class OnInteract : MonoBehaviour
{
    
    
    public enum Actions
    {
        Story,Item,Event        
    }
    
    public GameObject ObjectTextUI;

    public List<Actions> ActionsList;
    private Queue<Actions> ActionsQueue=new Queue<Actions>();
    private bool ActionDone=true;
    
    public List<String> TextPaths;
    private int TextIndex=0;
    
    public float offset=4.0f;
    public String PopupText = "按F互動";
    private Text popup;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            //When Player enter then show floating text on the object
            popup = ObjectTextUI.transform.Find("PopUpText").GetComponent<Text>();
            Vector2 pos = gameObject.transform.position;
            ObjectTextUI.transform.position = new Vector2(pos.x, pos.y + offset);
            popup.text = PopupText;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            Vector2 pos = gameObject.transform.position;
            ObjectTextUI.transform.position = new Vector2(pos.x, pos.y + offset);
            popup.text = PopupText;
            //If Player press F then Enqueue all action to the queue
            if (ActionsQueue.Count==0 && Input.GetKeyDown(KeyCode.F) )
            {
                TextIndex = 0;
                foreach (Actions action in ActionsList)
                {
                    ActionsQueue.Enqueue(action);
                }
            }
            //DoAction
            if (ActionsQueue.Count!=0)
            {
                popup.text = "";
                DoAction();
            }
        }
        
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            //When Player exit then disable floating text
            popup.text = "";
        }
    }
    
    
    public void DoAction()
    {   
        //Different Actions
        //Maybe I will make an event factory later
        if(!ActionDone) return;
        ActionDone = false;
        switch (ActionsQueue.Peek())
            {
                case Actions.Story:
                    ObjectTextUI
                        .GetComponent<TextUIScript>()
                        .LoadText(TextPaths[TextIndex],this);
                    TextIndex++;
                    break;
                case Actions.Item:
                    break;
                case Actions.Event:
                    break;
            }
    }

    public void SetActionDone()
    {
        ActionsQueue.Dequeue();
        ActionDone = true;
    }

    public OnInteract AddAction(Actions action,String path)
    {
        ActionsList.Add(action);
        TextPaths.Add(path);
        return this;
    }
    public OnInteract ClearAction()
    {
        ActionsList.Clear();
        TextPaths.Clear();
        return this;
    }

}
