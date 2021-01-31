using System;
using UnityEngine;

public class OnDialogue : MonoBehaviour
{ 
    public String TextPath;
    public TextUIScript TextUI;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Player"))
        {
            //Load Text then destroy the trigger object.
            TextUI.LoadText(TextPath);
            Destroy(this);
        }
    }
}