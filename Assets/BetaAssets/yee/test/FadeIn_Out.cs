using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn_Out : MonoBehaviour
{
    Image image;
    void Start()
    {
        image = GetComponent<Image>();
        StartCoroutine(FadeIn(5));
    }
    IEnumerator FadeIn(float UseTime)
    {
        float Timer = 0;
        Color color = image.color;
        color.a = 0;
        if(UseTime <= 0) 
        {
            color.a = 1;
            image.color = color;
            yield break;
        }
        while (Timer < UseTime)
        {
            Timer += Time.deltaTime;
            color.a = Mathf.Clamp(Timer / UseTime, 0, 1);
            image.color = color;
            yield return null;
        }
        yield break;
    }
    IEnumerator FadeOut(float UseTime)
    {
        yield break;
    }
}
