using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class teleport : MonoBehaviour
{
    public bool cantele = false;

    void Update()
    {
        if(cantele && Input.GetKeyDown(KeyCode.W))
        {
            SceneManager.LoadScene(3);
        }
    }
    void OnTriggerExit2D(Collider2D coll)
    {
        if(coll.tag == "Player")
            cantele = false;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.tag == "Player")
            cantele = true;
    }
}
