using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public static class Quest
{
    public static int[]                QuestsProgress; //Record where the player get on quest line
    public static QuestSavingClass[]   QuestState;     //Record whether quest is available or finished
    public static byte[] flags;                        //0:not set 1:set otherwise:disable
    public static void QuestFinished(int ID,int SubID)
    {
        QuestsProgress[ID]++;
        if(QuestsProgress[ID]>=Quests.QuestsNum[ID])
        {
            QuestState[ID].IsActive = false;
            QuestState[ID].IsAvailable = false;
            QuestState[ID].IsFinished = true;
            Debug.Log("Quest Completed : " + ID.ToString());
        }

        //return reward
    }

    public static QuestClass GetQuest(int ID,int SubID)
    {
        return Quests.QuestsFromJson[GetQuestIndex(ID,SubID)];
    }

    public static int GetProgress(int ID)
    {
        return QuestsProgress[ID];
    }
    public static QuestSavingClass GetQuestState(int ID)
    {
        return QuestState[ID];
    }

    public static bool QuestInit()
    {
        if (isInit) return false;
        isInit = true;
        string path = Path.Combine(Application.dataPath, "06.Data/Quests/Content/QuestsInfo.json");
        Quests = JsonUtility.FromJson<Quests>(File.ReadAllText(path));

        QuestsProgress = new int[Quests.QuestsNum.Length];
        QuestState = new QuestSavingClass[Quests.QuestsNum.Length];
        for(int i=0;i< Quests.QuestsNum.Length;i++)
        {
            QuestState[i] = new QuestSavingClass {IsActive = false,IsAvailable = false,IsFinished = false };
        }
        flags = new byte[flagnum=Enum.GetNames(typeof(FlagID)).Length];


        List<QuestDetailProto> objects = new List<QuestDetailProto>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(QuestDetailProto)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(QuestDetailProto))))
        {
            objects.Add((QuestDetailProto)Activator.CreateInstance(type));
        }
        questDetailProtos = objects.ToArray();
        return true;
    }

    public static void ActivateQuest(int ID)
    {
        if(QuestState[ID].IsAvailable)
        {
            QuestState[ID].IsActive = true;
        }
    }

    public static void DeactivateQuest(int ID)
    {
        QuestState[ID].IsActive = false;
    }

    public static void AvailQuest(int ID)
    {
        QuestState[ID].IsAvailable = true;
    }

    public static void UnavailQuest(int ID)
    {
        if (!QuestState[ID].IsActive)
        {
            QuestState[ID].IsAvailable = false;
        }
    }

    public static void SetFlag(FlagID flag)
    {
        if (flag == FlagID.NULL) { goto RunQuest; }
        //check whether the flag is enable
        if (flags[(int)flag] == 0)
        {
            flags[(byte)flag] = 1;
        }
        else if (flags[(int)flag] != 1){ return; }
        RunQuest:
        foreach (var q in questDetailProtos)
        {
            q.Run();
        }
    }
    public static void UnsetFlag(FlagID flag)
    {
        flags[(int)flag] = 0;
    }

    public static void DisableFlag(FlagID flag)
    {
        flags[(int)flag] = 2;
    }

    public static byte flag(FlagID flag)
    {
        return flags[(int)flag];
    }

    public static bool Check(int ID, int SubID)
    {
        return !QuestState[ID].IsActive || !QuestState[ID].IsAvailable || QuestState[ID].IsFinished ||QuestsProgress[ID]!=SubID;
    }

    //Private member
    private static Quests Quests;
    private static int flagnum;
    private static bool isInit=false;
    private static QuestDetailProto[] questDetailProtos
    //={new Quest_0_0_()}
    ;
    private static int GetQuestIndex(int ID,int SubID)
    {
        int Index = 0;
        for(int i=0;i<ID;i++)
        {
            Index += Quests.QuestsNum[i];
        }
        return Index+SubID;
    }
}