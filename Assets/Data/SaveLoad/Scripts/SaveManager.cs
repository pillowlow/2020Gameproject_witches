using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager manager = null;
    [SerializeField] private Button SaveButton;
    [SerializeField] private Button LoadButton;
    private void Awake()
    {
        manager = this;
        SaveButton.onClick.AddListener(SaveLoadButton.Save_Current_Saving);
        LoadButton.onClick.AddListener(SaveLoadButton.Load_Current_Scene_Data);
    }
    public enum Type
    {
        Door
    }
#if UNITY_EDITOR
    HashSet<int> id_set = new HashSet<int>();
    
#endif
    AllObjects objects = new AllObjects();
    Dictionary<int, GameObject> id_to_gameObject = new Dictionary<int, GameObject>();
    List<ResetTransform> resetObjects = new List<ResetTransform>();
    public void AddSaved(GameObject gameObject_In, Type type_In, int id_In)
    {
#if UNITY_EDITOR
        if(id_In < 0)
        {
            Debug.LogError("Id cannot be negative.");
            return;
        }
        if(id_set.Contains(id_In))
        {
            Debug.LogError("Save Object Id cannot be repetitive : ");
            Debug.LogError(gameObject_In.name + " and " + id_to_gameObject[id_In].name);
            return;
        }
        else
        {
            id_set.Add(id_In);
        }
#endif
        id_to_gameObject[id_In] = gameObject_In;
        switch (type_In)
        {
            case Type.Door:
                objects.doors.Add(new DoorClass(ref gameObject_In, id_In));
                break;
        }
    }

    public void AddReset(ResetTransform Object_In)
    {
        resetObjects.Add(Object_In);
    }

    public void ResetAll()
    {
        PlayerMovement.instance.LeaveAllState();
        foreach(var i in resetObjects)
        {
            i.ResetData();
        }
    }

    public void SaveAll(string path)
    {
        //Door
        foreach (var i in objects.doors)
        {
            i.Save(id_to_gameObject[i.id].GetComponent<Door>());
        }
        File.WriteAllText(path, JsonUtility.ToJson(objects));
    }

    public void LoadAll(string path)
    {
        objects = JsonUtility.FromJson<AllObjects>(File.ReadAllText(path));
        //Door
        foreach(var i in objects.doors)
        {
            i.Load(id_to_gameObject[i.id].GetComponent<Door>());
        }
    }

    [System.Serializable]
    class AllObjects
    {
        public List<DoorClass> doors = new List<DoorClass>();
    }

    [System.Serializable]
    class DoorClass
    {
        public DoorClass(ref GameObject object_In, int id_In)
        {
            id = id_In;
            Door door = object_In.GetComponent<Door>();
            isClose = door.isClosed;
        }
        public void Save(Door target)
        {
            isClose = target.isClosed;
        }
        public void Load(Door target)
        {
            if(!isClose)
            {
                target.OpenDoor(null);
            }
        }
        public int id = -1;
        public bool isClose = false;
    }
}
