[System.Serializable]
public class QuestClass
{
    public string Title;
    public string Description;
    //public ItemOrMoney Reward;
    //public FlagID[] FlagNeeded;
}
[System.Serializable]
public class QuestSavingClass
{
    public bool IsAvailable = false;
    public bool IsActive = false;
    public bool IsFinished = false;
}
[System.Serializable]
public class Quests
{
    public int[] QuestsNum;
    public QuestClass[] QuestsFromJson;
}

public abstract class QuestDetailProto
{
    public abstract void Run();
}
public enum FlagID
{
    NULL,Dialog_0_0, Visit_0_0
}