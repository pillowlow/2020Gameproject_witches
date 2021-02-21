using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryChangeType : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{

    [SerializeField]
    static GameObject Current;

    private void Awake()
    {
        if (Current == null) 
        {
            Current = gameObject;
            Current.GetComponent<RectTransform>().localScale = new Vector3(1.195f,1.195f,1);
        }
    }
    public void OnPointerEnter(PointerEventData e)
    {
        GetComponent<RectTransform>().localScale = new Vector3(1.5f,1.5f,1);
    }
    public void OnPointerExit(PointerEventData e)
    {
        if (ReferenceEquals( gameObject , Current))
        {
            GetComponent<RectTransform>().localScale = new Vector3(1.195f, 1.195f, 1);
        }
        else
        {
            GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }
    public void ChangeType(int type)
    {
        if (ReferenceEquals( gameObject , Current)) { return; }
        Current.GetComponent<RectTransform>().localScale = Vector3.one;
        Inventory.Current = (Inventory.ItemType)type;
        Inventory.UpdateUI();
        Current = gameObject;
    }
}
