using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Pathfinding.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class objectTextUI : MonoBehaviour
{
    private OnInteract Instance;
    public GameObject UI;
    private List<String> Text;
    private bool active;
    private int index;
    // Start is called before the first frame update
    void Start()
    {
        active = false;
        UI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            if (index >= Text.Count)
            {
                
                active = false;
                UI.SetActive(false);
                index = 0;
                Instance.active = true;
                Instance = null;
            }
            else
            {
                UI.GetComponentInChildren<Text>().text = Text[index];
                if (Input.GetKeyDown(KeyCode.H))
                {
                    index++;
                }
            }
            
        }
    }

    public void LoadText(String path,OnInteract instance)
    {
        Instance = instance;
        using (StreamReader r = new StreamReader(Application.dataPath+"/02.UI/StoryScripts/"+path))
        {
            string json = r.ReadToEnd();
            Text = (List<String>)TinyJsonDeserializer.Deserialize(json,typeof(List<String>));
            index = 0;
            active = true;
            UI.SetActive(true);
        }
    }
}
