using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTransform : MonoBehaviour
{
    private Vector3 OriginalPosition;
    private Quaternion OriginalRotation;
    private Transform parent;
    private void Start()
    {
        OriginalPosition = transform.position;
        OriginalRotation = transform.rotation;
        parent = transform.parent;
        SaveManager.manager.AddReset(this);
    }
    public void ResetData()
    {
        transform.SetPositionAndRotation(OriginalPosition, OriginalRotation);
        transform.parent = parent;
    }
}
