using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveLoadButton
{

    //public GameObject Player;
    //Call it when "New Game" is clicked
    public static void NewGame()
    {
        SaveLoad.SetSavingSlot(SaveLoad.GetSavingNum());
        Quest.QuestInit();
    }
    public static void Load_Button(int x)
    {
        SaveLoad.SetSavingSlot(x);
        Load_Current_Saving();
    }

    public static void Save_Button(int s)
    {
        SaveLoad.SetSavingSlot(s);
        Save_Current_Saving();
    }
    //Quick Save
    public static void Save_Current_Saving()
    {
        //SaveLoad.Save((PlayerData.SceneID)Enum.Parse(typeof(PlayerData.SceneID), SceneManager.GetActiveScene().name));
        SaveLoad.Save(PlayerData.SceneID.scene_01_01);
    }
    //Quick Load
    public static void Load_Current_Saving()
    {
        SaveLoad.Load();
    }

    public static void Load_Current_Scene_Data()
    {
        SaveLoad.LoadSceneData(PlayerData.SceneID.scene_01_01);
        //SaveLoad.LoadSceneData((PlayerData.SceneID)Enum.Parse(typeof(PlayerData.SceneID), SceneManager.GetActiveScene().name));
    }

}
