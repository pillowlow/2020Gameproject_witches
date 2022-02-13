using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI: MonoBehaviour
{

    private float _soundEffect;
    private float _bgm;
    public Text soundEffectText;
    public Text bgmText;
    public void SetSoundEffectVolume(float num)
    {
        _soundEffect = num;
        soundEffectText.text = Math.Round(_soundEffect*100) + "%";
    }

    public void SetBGMVolume(float num)
    {
        _bgm = num;
        bgmText.text = Math.Round(_bgm*100) + "%";
    }
}