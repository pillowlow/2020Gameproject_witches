using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class SaveLoad
{
    public static void Save(PlayerData.SceneID SceneID)
    {
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

        string path = Path.Combine(Application.persistentDataPath, "Save_" + Saving.ToString() + ".json");
        File.WriteAllText(path,JsonUtility.ToJson(Data));
    }

    public static void LoadScene()
    {
        string path = Path.Combine(Application.persistentDataPath, "Save_" + Saving.ToString()+".json");
        PlayerData Data = JsonUtility.FromJson<PlayerData>(File.ReadAllText(path));                                 //Get data from disk
        SceneManager.LoadScene(Data.CurrentScene.ToString());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public static string GetSavingDateTime(int s)
    {
        string path = Path.Combine(Application.persistentDataPath, "Save_" + s.ToString() + ".json");
        PlayerData Data = JsonUtility.FromJson<PlayerData>(File.ReadAllText(path));
        return Data.Time;
    }
    public static void SetSpawnPos(Vector2 Pos)    //call it when triger check point
    {
        SpawnPos = Pos;
    }
    public static void SetSaving(int s)
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
        string path = Path.Combine(Application.persistentDataPath, "Save_" + Saving.ToString() + ".json");
        PlayerData Data = JsonUtility.FromJson<PlayerData>(File.ReadAllText(path));                                 //Get data from disk
        GameObject Player = GameObject.Find("Player");

        Quest.QuestsProgress = Data.QuestProgress;
        Quest.QuestState = Data.QuestState;
        Quest.flags = Data.Flags;
        Vector2 Pos = GetSpawnPosition(Data.CurrentScene);        
        Player.transform.position = new Vector3(Pos.x, Pos.y, 0.0f);
        PlayerManager.state = PlayerManager.StateCode.idle; //Idk why but it fixs sth
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
