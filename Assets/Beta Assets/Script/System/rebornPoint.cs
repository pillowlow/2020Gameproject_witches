using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class rebornPoint : MonoBehaviour
{
    public GameObject player;
    public string rebornScene;
    public string rebornLocation;
    
    
    // Start is called before the first frame update
    void Start()
    {
        player.GetComponent<playerUnit>().rebornScene = SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
