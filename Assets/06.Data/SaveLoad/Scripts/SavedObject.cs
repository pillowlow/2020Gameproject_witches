using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedObject : MonoBehaviour
{
    [SerializeField] private SaveManager.Type type;
    [SerializeField] private int Id = -1;
    private void Start()
    {
        SaveManager.manager.AddSaved(gameObject, type, Id);
    }
}
