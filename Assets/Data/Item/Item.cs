using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class Item
{
    [SerializeField]
    private string id;
    [SerializeField]
    private string name;
    [SerializeField]
    private string description;
    [SerializeField]
    private string type;
    [SerializeField]
    private string image;
    
    private static Dictionary<string, Item> _dictionary = new Dictionary<string, Item>();
    
    
    static Item()
    {
        string text = Resources.Load<TextAsset>("Item/Item").ToString();
        Item[] items = JsonHelper.FromJson<Item>(text);
        foreach (var i in items)
        {
            _dictionary.Add(i.id,i);
        }
    }
    
    public string GetID()
    {
        return id;
    }
    
    public string GetName()
    {
        return name;
    }

    public string GetDescription()
    {
        return description;
    }

    public string GetItemType()
    {
        return type;
    }

    public Sprite GetItemImage()
    {
        return Resources.Load<Sprite>("Item/Image/"+image);
    }
    

    public static Item GetItemById(string id)
    {
        if (!_dictionary.ContainsKey(id)) return null;
        return _dictionary[id];
    } 
    
    
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

}