public class PlayerData
{
    [System.Serializable]
    public enum SceneID
    {
        scene_01_01,
        scene_01_02,
        scene_01_03,
        scene_01_04,
    };
    public string Version = "";
    public string Time;
    public SceneID CurrentScene = SceneID.scene_01_01;
    public float x_Position = 0.0f;
    public float y_Position = 0.0f;
    public int[] QuestProgress;
    public QuestSavingClass[] QuestState;
    public byte[] Flags;
}
