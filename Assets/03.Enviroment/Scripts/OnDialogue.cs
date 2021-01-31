using System;
using UnityEngine;

public class OnDialouge : MonoBehaviour
{ 
    public String LoadPath;
    public objectTextUI UI;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            UI.LoadText(LoadPath);
            Destroy(this);
        }
    }
}