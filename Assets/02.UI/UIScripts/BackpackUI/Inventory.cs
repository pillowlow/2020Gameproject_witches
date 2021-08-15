using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Inventory
{
    
    static LinkedList<Item>[] Items =
    {
        new LinkedList<Item>(),
        new LinkedList<Item>(),
        new LinkedList<Item>()
    };

    //Remove item successfully with return code 0. Failed to remove item due to not enough item with return code -1.
    public static int RemoveItem(int index)
    {
        return 0;
    }
    

    public static void AddItem(Item item)
    {
        if (item != null)
        {
            string type = item.GetItemType();
            int page = (int) Enum.Parse(typeof(InventoryUI.Page), type);
            Items[page].AddLast(item);
        }
        else
        {
            Debug.LogError("You can't add a empty item");
        }
    }
    
    
    //Check if Player has Item by id
    public static Item GetItemByIndex(InventoryUI.Page page, int index)
    {
        if (index >= Items[(int)page].Count) return null;
        return Items[(int)page].ElementAt(index);
    }

    //Check if Player has Item by id
    public static bool HasItem(string id)
    {
        foreach (LinkedList<Item> list in Items)
        {
            foreach (Item item in list)
            {
                if (item.GetID() == id) return true;
            }
        }
        return false;
    }
    
    //Check if Player has Item by id and type
    public static bool HasItem(string id, InventoryUI.Page p)
    {
        foreach (Item item in Items[(int)p])
        {
            if (item.GetID() == id) return true;
        }
        return false;
    }

    public static LinkedList<Item> GetItemList(InventoryUI.Page p)
    {
        return Items[(int)p];
    }

    public static List<Item> SearchItems()
    {

        return null;
    }
        
}
