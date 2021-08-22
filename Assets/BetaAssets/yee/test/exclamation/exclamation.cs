using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class exclamation : MonoBehaviour
{
    [SerializeField] GameObject CheckMessage;
    [SerializeField] Text MessageText;
    [SerializeField] string Message;
    [SerializeField] GameObject Sprite;
    [SerializeField] string item_id;
    bool StayIn = false;
    bool Picked = false; //save

    void OnTriggerEnter2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12 || Picked) { return; }
        StayIn = true;
        Sprite.SetActive(true);
    }
    void OnTriggerExit2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12) { return; }
        StayIn = false;
        Sprite.SetActive(false);
    }
    public void OnMouseDown()
    {
        if(!StayIn) { return; }
        CheckMessage.SetActive(true);
        MessageText.text = Message;
    }
    public void CheckPick()
    {
        Inventory.AddItem(Item.GetItemById(item_id));
        Picked = true;
        StayIn = false;
        Sprite.SetActive(false);
        CheckMessage.SetActive(false);
    }
}
