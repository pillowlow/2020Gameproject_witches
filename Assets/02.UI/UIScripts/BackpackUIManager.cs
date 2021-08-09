using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackpackUIManager : MonoBehaviour
{
    private static int _page;
    [SerializeField]
    private List<GameObject> UI = new List<GameObject>();

    public static BackpackUIManager Instance;
    private static PlayerManager playerManager;
    private static bool _isOpened = false;
    private void Awake()
    {
        foreach (GameObject ui in UI)
        {
            ui.SetActive(false);
        }
    }

    private void Start()
    {
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
                PlayerManager.SetState(PlayerManager.StateCode.Stop);
                _isOpened = true;
                UI[_page].SetActive(true);
            }
            else
            {
                PlayerManager.SetState(PlayerManager.StateCode.Idle);
                _isOpened = false;
                UI[_page].SetActive(false);
            }
        }
    }

    public void OpenBackpackByButton(bool open)
    {
        if (open && !_isOpened)
        {
            PlayerManager.SetState(PlayerManager.StateCode.Stop);
            _isOpened = true;
            UI[_page].SetActive(true);
        }else if (!open && _isOpened)
        {
            PlayerManager.SetState(PlayerManager.StateCode.Idle);
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
}