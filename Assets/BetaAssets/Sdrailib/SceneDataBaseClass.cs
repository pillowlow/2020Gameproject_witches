using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
abstract public class SceneDataHolderBaseClass : MonoBehaviour
{
    public GameObject Player;
    public CameraController Camera;
    abstract public SceneDataBaseClass Save();
    abstract public void Load(SceneDataBaseClass d);
}

[System.Serializable]
abstract public class SceneDataBaseClass
{
    public Vector3 PlayerPosition;
    public Vector3 CameraPosition;
}

