
using System.Collections.Generic;
using UnityEngine;

public class BackpackUIManager : MonoBehaviour
{
    private static int _page;
    [SerializeField]
    private List<GameObject> UI = new List<GameObject>();

    public static BackpackUIManager Instance;
    private static PlayerManager playerManager;
    private bool _isOpened = false;

    enum Page
    {
        Inventory=0,Setting=1
    }
    
    private void Awake()
    {
        foreach (GameObject ui in UI)
        {
            ui.SetActive(false);
        }
    }

    private void Start()
    {
        Inventory.AddItem(Item.GetItemById("iron_gate_key"));
        Inventory.AddItem(Item.GetItemById("sword_of_thousand_truth"));
        Inventory.AddItem(Item.GetItemById("letter_from_past"));
        Instance = this;
        playerManager = PlayerManager.instance;
    }

    private void Update()
    {
        OpenBackpack();
    }

    private void OpenBackpack()
    {
        if (playerManager.input.GetKeyDown(InputAction.Backpack))
        {
            if (!_isOpened)
            {
                PlayerMovement.SetContinue(false);
                _isOpened = true;
                UI[_page].SetActive(true);
            }
            else
            {
                PlayerMovement.SetContinue(true);
                _isOpened = false;
                UI[_page].SetActive(false);
            }
        }
    }

    public void OpenBackpackByButton(bool open)
    {
        if (open && !_isOpened)
        {
            PlayerMovement.SetContinue(false);
            _isOpened = true;
            UI[_page].SetActive(true);
        }else if (!open && _isOpened)
        {
            PlayerMovement.SetContinue(true);
            _isOpened = false;
            UI[_page].SetActive(false);
        }
    }

    public void AddPage(int num)
    {
        int t = _page + num;
        if (!(t>=UI.Count || t<0))
        {
            UI[_page].SetActive(false);
            _page = t;
            UI[_page].SetActive(true);
        }
    }

    public int GetPage() { return _page; }

    public bool IsOpen()
    {
        return _isOpened;
    }
}