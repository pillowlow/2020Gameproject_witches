﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestVillager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(Quest.QuestInit())
        {
            Quest.QuestState[0] = new QuestSavingClass { IsActive = true, IsAvailable = true, IsFinished = false };//Enable and activate the first quest
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (!Input.GetKeyDown(KeyCode.F)) { return; }
        
        switch(this.name)
        {
            case "Villager":
            {
                if (Quest.flag(FlagID.Visit_0_0) == 1)
                {
                    Debug.Log("ConversationB with the villager");
                    GetComponent<OnInteract>().TextPath = "test.json";
                }
                else
                {
                    Debug.Log("ConversationA with the villager");
                    GetComponent<OnInteract>().TextPath = "rock.json";
                    Quest.SetFlag(FlagID.Dialog_0_0);
                }
                break;
            }
            case "CrystalBall":
            {
                if(Quest.flag(FlagID.Dialog_0_0)==1)
                {
                    Debug.Log("You Found a Mysterious Crystal Ball...");
                    Quest.SetFlag(FlagID.Visit_0_0);
                }
                break;
            }
        }
    }
}

public class Quest_Villager : QuestDetailProto
{
    public override void Run()
    {
        //var state = Quest.GetQuestState(0);
        if (Quest.Check(0, 0)) { return; }
        if (Quest.flag(FlagID.Dialog_0_0) == 1)
        {
            Debug.Log("Quest Completed! : Talk With The Villager.");
            Debug.Log("Conversation...");
            Debug.Log("Quest Accepted! : Find The Mysterious Crystal.");
            Quest.QuestFinished(0, 0);
            Quest.AvailQuest(0);
            Quest.ActivateQuest(0);
        }
    }
}