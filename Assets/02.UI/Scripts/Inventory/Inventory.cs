using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class Inventory
{
    static InventoryUI UI;
    static List<GameObject> Item = new List<GameObject>();
    public static ItemType Current = ItemType.Props;
    public enum ItemType
    {
        Props,Material,CrucialPros,Tattoo
    }
    [System.Serializable]
    public class Item_T
    {
        public int ID;
        public int Num;
    }
    [System.Serializable]
    public class SubInv_T
    {
        public SubInv_T()
        {
            Item = new List<Item_T>();
        }
        const int InventoryMaxSize = 16;
        public List<Item_T> Item;
        public int GetItem(int ID)
        {
            for(int i=0;i<Item.Count;i++)
            {
                if (Item[i].ID == ID) { return i; }
            }
            if(Item.Count>=InventoryMaxSize-1)
            {
                return -1;
            }
            return Item.Count;
        }
    }

    [System.Serializable]
    public class TipInfo
    {
        public int id;
        public string name;
        public string system_name;
        public string description;
    }
    class Wrapper<T>
    {
        public T[] data;
    }
    public static TipInfo[] Tips;
    public static SubInv_T[] SubInv = new SubInv_T[4] {new SubInv_T(), new SubInv_T(), new SubInv_T(), new SubInv_T()};

    //Get item successfully with return code 0. Failed to get item due to full inventory with return code -1.
    public static int GetItem(ItemType Type,int ItemID,int num)
    {
        int index = SubInv[(int)Type].GetItem(ItemID);        
        if(index==-1)
        {
            return -1;
        }
        if (index == SubInv[(int)Type].Item.Count)
        {
            SubInv[(int)Type].Item.Add(new Item_T {ID = ItemID, Num = 0 });
        }
        SubInv[(int)Type].Item[index].Num+=num;
        if (Type == Current)
        {
            UpdateUI();
        }
        return 0;
    }

    //Remove item successfully with return code 0. Failed to remove item due to not enough item with return code -1.
    public static int RemoveItem(ItemType Type, int ItemID, int num)
    {
        int index = SubInv[(int)Type].GetItem(ItemID);
        if (index == -1)
        {
            return -1;
        }
        if(SubInv[(int)Type].Item[index].Num>=num)
        {
            SubInv[(int)Type].Item[index].Num -= num;
            if(SubInv[(int)Type].Item[index].Num==0)
            {
                SubInv[(int)Type].Item.RemoveAt(index);
            }
        }
        else
        {
            return -1;
        }
        if (Type == Current)
        {
            UpdateUI();
        }
        return 0;
    }
    public static void InitInv(InventoryUI ui)
    {
        UI = ui;
        if (Tips == null)
        {
            string path = Path.Combine(Application.dataPath, "06.Data/ItemDescription/TipDesc.json");
            string text = File.ReadAllText(path,System.Text.Encoding.UTF8);
            Tips = JsonUtility.FromJson<Wrapper<TipInfo>>(text).data;
        }
        GetItem(ItemType.Props, 0, 1);
    }

    //Get item id in the inventory at coord (x,y)
    public static int GetItem(int x,int y)
    {
        int Index = 4 * y + x;
        if(SubInv[(int)Current].Item.Count>Index)
        {
            return SubInv[(int)Current].Item[Index].ID;
        }
        return -1;
    }
    public static void UpdateUI()
    {

        foreach(GameObject i in Item)
        {
            UI.DestroyThis(i);
        }
        Item.Clear();
        const float slotx = -653.5f;
        const float sloty = 195.0f;
        const float dis = 137.0f;
        float offsety = sloty;
        for (int i = 0; i < 4; i++, offsety -= dis)
        {
            float offsetx = slotx;
            for (int j = 0; j < 4; j++, offsetx += dis)
            {
                if (i * 4 + j >= SubInv[(int)Current].Item.Count) { return; }
                Vector2 Pos = new Vector3(offsetx, offsety);
                int Index = i * 4 + j;
                Item.Add(UI.CreateImg(GetItemName(SubInv[(int)Current].Item[Index].ID,true), Pos, SubInv[(int)Current].Item[Index].Num));
            }
        }
    }
    public static string GetItemName(int ItemID,bool system = false)
    {
        int index = ItemID;
        while (ItemID != Tips[index].id)
        {
            index--;
            if (index <= 0)
            {
                Debug.Log("Item Does Not Exist :" + ItemID.ToString());
                return "";
            }
        }
        return system ? Tips[index].system_name : Tips[index].name;
    }
}
