using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SaveLoadButton : MonoBehaviour
{
    public GameObject Player;
    //Call it when "New Game" is clicked
    public void NewGame()
    {
        SaveLoad.SetSaving(GetSavingNum());
        Quest.QuestInit();
    }
    public void Load_Button(int x)
    {
        SaveLoad.SetSaving(x);
        Load_Current_Saving();
    }

    public void Save_Button(int x)
    {
        SaveLoad.SetSaving(x);
        Save_Current_Saving();
    }
    //Quick Save
    public void Save_Current_Saving()
    {
        SaveLoad.Save((PlayerData.SceneID)Enum.Parse(typeof(PlayerData.SceneID), SceneManager.GetActiveScene().name));
    }
    //Quick Load
    public void Load_Current_Saving()
    {
        SaveLoad.LoadScene();
    }

    //Get the number of savings
    public int GetSavingNum()
    {
        string path = Path.Combine(Application.persistentDataPath, "Save_0.json");
        int s = 1;
        for (; File.Exists(path); s++)
        {
            path = Path.Combine(Application.persistentDataPath, "Save_" + s.ToString() + ".json");
        }
        return s-1;
    }
}
