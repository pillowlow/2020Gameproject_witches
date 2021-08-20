using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    
    public enum Page
    {
        Magical = 0, Important=1, Badge=2
    }

    private Page CurrentType = Page.Magical;
    private int CurrentPage = 0;
    private int CurrentIndex = 0;
    public GameObject ItemPage;
    public GameObject Title;
    public GameObject Description;
    private List<GameObject> ItemIcons = new List<GameObject>();
    private bool IsOpen = false;
    int Focusx = 0;
    int FocusY = 0;
    [SerializeField]
    TMP_InputField SearchBar;

    public void Awake()
    {
        foreach (Transform child in ItemPage.transform)
        {
            ItemIcons.Add(child.gameObject);
        }
    }

    private void OnEnable()
    {
        UpdateInventory();
    }

    private void UpdateInventory(LinkedList<Item> DisplayItems = null)
    {
        if(DisplayItems == null) DisplayItems = Inventory.GetItemList(CurrentType);
        ClearInventory();
        int i = 0;
        foreach (Item item in DisplayItems)
        {
            GameObject icon = ItemIcons[i];
            icon.SetActive(true);
            icon.GetComponentInChildren<TextMeshProUGUI>().text = item.GetName();
            icon.transform.Find("IconBG").Find("Icon").GetComponent<Image>().sprite = item.GetItemImage();
            i++;
        }
    }

    public void ClearInventory()
    {
        foreach (GameObject icon in ItemIcons)
        {
            icon.GetComponentInChildren<TextMeshProUGUI>().text = "";
            icon.transform.Find("IconBG").Find("Icon").GetComponent<Image>().sprite = null;
            icon.SetActive(false);
        }
    }

    public void ChangeType(int num)
    {
        int t = (int)CurrentType + num;
        if (t > 2) t = 0;
        else if (t < 0) t = 2;
        switch (t)
        {
            case 0:
                CurrentType = Page.Magical;
                break;
            case 1:
                CurrentType = Page.Important;
                break;
            case 2:
                CurrentType = Page.Badge;
                break;
        }
        CurrentIndex = 0;
        ShowItem();
        UpdateInventory();
    }
    
    
    public void SelectItem(int index)
    {
        CurrentIndex = index + CurrentPage * 12;
        ShowItem();
    }
    public void ShowItem()
    {
        Item item = Inventory.GetItemByIndex(CurrentType,CurrentIndex);
        if (item != null)
        {
            Title.GetComponent<TextMeshProUGUI>().text = item.GetName();
            Description.GetComponent<TextMeshProUGUI>().text = item.GetDescription();
        }
        else
        {
            Title.GetComponent<TextMeshProUGUI>().text = "";
            Description.GetComponent<TextMeshProUGUI>().text = "";
        }
    }
    
    public void SearchItems()
    {
        ClearInventory();
        UpdateInventory(Inventory.SearchItems(SearchBar.text));
    }

}
