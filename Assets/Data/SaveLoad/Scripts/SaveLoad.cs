using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class SaveLoad
{
    public static void Save(PlayerData.SceneID SceneID)
    {
        SaveScene(SceneID);
        Vector2 Pos = GetSpawnPosition(SceneID);
        PlayerData Data = new PlayerData
        {
            Time = DateTime.Now.ToString(),
            CurrentScene = SceneID,
            x_Position = Pos.x,
            y_Position = Pos.y,
            QuestProgress = Quest.QuestsProgress,
            QuestState = Quest.QuestState,
            Flags = Quest.flags
        };

        string path = GetSavingDirectory(Saving);
        path = Path.Combine(path, "CharacterData");
        File.WriteAllText(path,JsonUtility.ToJson(Data));
    }

    public static void Load()
    {
        string path = GetSavingDirectory(Saving);
        path = Path.Combine(path, "CharacterData");
        PlayerData Data = JsonUtility.FromJson<PlayerData>(File.ReadAllText(path));                                 //Get data from disk
        SceneManager.LoadScene(Data.CurrentScene.ToString());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public static string GetSavingDateTime(int s)
    {
        string path = GetSavingDirectory(s);
        path = Path.Combine(path, "CharacterData");
        PlayerData Data = JsonUtility.FromJson<PlayerData>(File.ReadAllText(path));
        return Data.Time;
    }
    public static void SetSpawnPos(Vector2 Pos)    //call it when triger check point
    {
        SpawnPos = Pos;
    }
    public static void SetSavingSlot(int s)
    {
        Saving = s;
    }
    //Private member
    private static Vector2 SpawnPos = new Vector2(-8192, -8192);
    private static int Saving = 0;
    private static Vector2 GetSpawnPosition(PlayerData.SceneID SceneID)
    {
        if (SpawnPos.x != -8192 && SpawnPos.y != -8192) { return SpawnPos; }        //Get Spawn Coordinate
        ;
        switch(SceneID)                                                             //Get default Spawn Coordinate
        {
            case PlayerData.SceneID.scene_01_01: {return new Vector2(18.0f, 8.5f); }
        }
        return new Vector2(0, 0);
    }
    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string path = GetSavingDirectory(Saving);
        path = Path.Combine(path, "CharacterData");
        PlayerData Data = JsonUtility.FromJson<PlayerData>(File.ReadAllText(path));                                 //Get data from disk
        GameObject Player = GameObject.FindWithTag("Player");

        Quest.QuestsProgress = Data.QuestProgress;
        Quest.QuestState = Data.QuestState;
        Quest.flags = Data.Flags;
        Vector2 Pos = GetSpawnPosition(Data.CurrentScene);        
        Player.transform.position = new Vector3(Pos.x, Pos.y, 0.0f);
        PlayerManager.state = PlayerManager.StateCode.Idle;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        //LoadScene
        GameObject SceneDataHolder = GameObject.FindWithTag("SceneDataHolder");
        LoadSceneData(Data.CurrentScene);
    }

    public static void SaveScene(PlayerData.SceneID SceneID)
    {
        //Save To The Disk
        string path = GetSavingDirectory(Saving);
        string file = Enum.GetName(typeof(PlayerData.SceneID), SceneID) + Saving.ToString();
        //file do hash
        path = Path.Combine(path, file);
        SaveManager.manager.SaveAll(path);
    }

    public static void LoadSceneData(PlayerData.SceneID SceneID)
    {
        string path = GetSavingDirectory(Saving);
        string file = Enum.GetName(typeof(PlayerData.SceneID), SceneID) + Saving.ToString();
        //file do hash
        path = Path.Combine(path, file);
        if(File.Exists(path))
        {
            SaveManager.manager.LoadAll(path);
            SaveManager.manager.ResetAll();
        }
    }

    //Get the number of savings
    public static int GetSavingNum()
    {
        string pre = Path.Combine(Application.persistentDataPath, "Save_");
        int s = 0;
        string path;
        do
        {
            path = pre + s.ToString();
        }while (File.Exists(path));
        return s;
    }

    public static string GetSavingDirectory(int s)
    {
        string path = Path.Combine(Application.persistentDataPath, "Save_" + s.ToString());
        System.IO.Directory.CreateDirectory(path);
        return path;
    }

}