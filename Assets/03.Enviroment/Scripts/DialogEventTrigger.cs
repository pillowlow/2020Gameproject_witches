using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (BoxCollider2D))]
public class DialogEventTrigger : MonoBehaviour
{
    [SerializeField] private string _showText = "";

    private BoxCollider2D _boxCollider2D = null;

    private void Awake()
    {
        _boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.CompareTag("Player")&&!string.IsNullOrEmpty(_showText)){
            Dialog.Instance.ShowTextArea(_showText);
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if(coll.CompareTag("Player")){
            Destroy(gameObject);
            Dialog.Instance.HideTextArea();
        }
    }
}
