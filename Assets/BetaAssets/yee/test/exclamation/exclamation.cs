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
    bool Picked = false; //save
    Collider2D collider2D;

    void Start()
    {
        collider2D = GetComponent<Collider2D>();
        if(collider2D == null) Debug.Log(1);
        Debug.Log(1);
    }
    void OnTriggerEnter2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12 || Picked) { return; }
        Sprite.SetActive(true);
    }
    void OnTriggerExit2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12) { return; }
        Sprite.SetActive(false);
    }
    void OnTriggerStay2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12 || Picked) { return; }
        if(PlayerManager.instance.input.Investigate(collider2D))
        {
            Click();
        }
    }

    public void Click()
    {
        CheckMessage.SetActive(true);
        MessageText.text = Message;
    }

    public void CheckPick()
    {
        Inventory.AddItem(Item.GetItemById(item_id));
        Picked = true;
        Sprite.SetActive(false);
        CheckMessage.SetActive(false);
    }
}
