using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string name;
    //public int fullNumber;
    public int id;
    public int type;
    public bool useable;
    public string info;

    private playerInventory Inventory;
    public GameObject itemButton;
    private void Start()
    {
        //Inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<playerInventory>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            for (int i = 0; i < Inventory.slots.Length; i++)
            {
                
                if (Inventory.isFull[i]==false)
                {
                    Inventory.isFull[i] = true;
                    Inventory.name[i] = name;
                    Inventory.id[i] = id;
                    Inventory.type[i] = type;
                    Inventory.useable[i] = useable;
                    Inventory.info[i] = info;
                    Instantiate(itemButton, Inventory.slots[i].transform, false);
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }
}
