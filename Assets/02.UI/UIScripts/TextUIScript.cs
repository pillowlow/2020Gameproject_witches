using System;
using System.Collections.Generic;
using System.IO;
using Pathfinding.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class TextUIScript : MonoBehaviour
{
    
    private OnInteract Instance;
    public GameObject UI;
    private Text TextUI;
    private Image Image;
    private List<String> Text;
    private bool active;
    private int index;
    // Start is called before the first frame update
    void Start()
    {
        //Get the Text and Image component and disable them
        TextUI = UI.transform.Find("Text").GetComponent<Text>();
        Image = UI.transform.GetComponentInChildren<Image>();
        active = false;
        TextUI.gameObject.SetActive(false);
        Image.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            //If index bigger than lines then close the TextUI
            if (index >= Text.Count)
            {
                active = false;
                TextUI.gameObject.SetActive(false);
                Image.gameObject.SetActive(false);
                index = 0;
                if (Instance)
                {
                    Instance.SetEventDone(true);
                    Instance = null;
                }
            }
            else
            //Press H to get the next line.
            {
                
                TextUI.text = Text[index];
                if (Input.GetKeyDown(KeyCode.H))
                {
                    index++;
                }
            }
            
        }
    }

    //For OnInteract Script to load Json file
    public void LoadText(String path,OnInteract instance)
    {
        Instance = instance;
        using (StreamReader r = new StreamReader(Application.dataPath+"/06.Data/StoryScripts/"+path+".json"))
        {
            string json = r.ReadToEnd();
            Text = (List<String>)TinyJsonDeserializer.Deserialize(json,typeof(List<String>));
            index = 0;
            active = true;
            TextUI.gameObject.SetActive(true);
            Image.gameObject.SetActive(true);
        }
    }
    //For OnDialogue Script to load Json file
    public void LoadText(String path)
    {
        using (StreamReader r = new StreamReader(Application.dataPath+"/06.Data/StoryScripts/"+path+".json"))
        {
            string json = r.ReadToEnd();
            Text = (List<String>)TinyJsonDeserializer.Deserialize(json,typeof(List<String>));
            index = 0;
            active = true;
            TextUI.gameObject.SetActive(true);
            Image.gameObject.SetActive(true);
        }
    }
}
