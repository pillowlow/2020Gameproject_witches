using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleport : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(this.name)
        {
            case "Door1-2": { SceneManager.LoadSceneAsync("Scene2"); break; }
            case "Door2-1": { SceneManager.LoadSceneAsync("Scene1"); break; }
        }
    }
}
