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
        SaveLoad.SetSaving(SaveLoad.GetSavingNum());
        Quest.QuestInit();
    }
    public static void Load_Button(int x)
    {
        SaveLoad.SetSaving(x);
        Load_Current_Saving();
    }

    public static void Save_Button(int s, SceneDataHolderBaseClass d)
    {
        SaveLoad.SetSaving(s);
        Save_Current_Saving(d);
    }
    //Quick Save
    public static void Save_Current_Saving(SceneDataHolderBaseClass x)
    {
        //SaveLoad.Save((PlayerData.SceneID)Enum.Parse(typeof(PlayerData.SceneID), SceneManager.GetActiveScene().name),x);
        SaveLoad.Save(PlayerData.SceneID.scene_01_01, x);
    }
    //Quick Load
    public static void Load_Current_Saving()
    {
        SaveLoad.Load();
    }


}
