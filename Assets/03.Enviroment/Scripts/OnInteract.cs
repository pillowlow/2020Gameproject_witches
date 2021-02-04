﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class OnInteract : MonoBehaviour
{
    
    
    public enum Actions
    {
        Story,Item,Quest,Event        
    }
    
    public GameObject ObjectTextUI;

    public List<Actions> ActionsList;
    public Queue<Actions> ActionsQueue=new Queue<Actions>();
    private bool ActionDone=true;
    
    public List<String> TextPaths;
    private int TextIndex=0;

    public List<String> FlagIds;
    
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
        String path = TextPaths[TextIndex];
        switch (ActionsQueue.Peek())
            {
                case Actions.Story:
                    CustomEventFactory
                        .GetEvent<LoadTextEvent, TextUIScript>(ObjectTextUI.GetComponent<TextUIScript>(), path)
                        .StartEvent(this);
                    TextIndex++;
                    break;
                case Actions.Item:
                    break;
                case Actions.Quest:
                    CustomEventFactory
                        .GetEvent<AcceptQuestEvent, QuestDetailProto>(null, path)
                        .StartEvent(this);
                    TextIndex++;
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
