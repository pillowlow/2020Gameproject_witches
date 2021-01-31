using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    private Dialog _instance = null;

    public Dialog Instance => _instance;

    [SerializeField] private Text _textArea = null;
    private void Awake()
    {
        _instance = this;
    }
}
