using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{    
    [SerializeField] private Image _hpBar = null;
    [SerializeField] private Image _sanBar = null;
    void Update()
    {
        if (_sanBar != null)
        {
            _sanBar.fillAmount = PlayerManager.sanityValue/100.0f;
        }

        if (_hpBar != null)
        {
            _hpBar.fillAmount = PlayerManager.hp / 100.0f;
        }
        
    }
}
