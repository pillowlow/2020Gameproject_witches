using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    private static Dialog _instance = null;

    public static Dialog Instance => _instance;
    
    [SerializeField] private Text _textArea = null;
    private void Awake()
    {
        _instance = this;
    }

    public void ShowTextArea(string text)
    {
        transform.GetComponent<RectTransform>().DOAnchorPosY( 300.0f, 0.5f);
        _textArea.text = text;
    }

    public void HideTextArea()
    {
        transform.GetComponent<RectTransform>().DOAnchorPosY( 0.0f, 0.3f);
        _textArea.text = "";
    }
}
