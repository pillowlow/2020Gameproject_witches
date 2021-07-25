using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneDataHolder01_01 : SceneDataHolderBaseClass
{
  
    public Door Door1;
    public Button saveButton;
    public Button loadButton;
    private void Awake()
    {
        saveButton.onClick.AddListener(()=> 
        {
            SaveLoadButton.Save_Button(0, this);
        });
        loadButton.onClick.AddListener(()=> 
        {
            //Reset the current scene
            SceneData01_01 d = SaveLoad.LoadScene(PlayerData.SceneID.scene_01_01) as SceneData01_01;
            Load(d);


            //Reload the scene
            //SaveLoadButton.Load_Button(0);
        });
    }
    public override SceneDataBaseClass Save()
    {
        SceneData01_01 data = new SceneData01_01
        {
            PlayerPosition = Player.transform.position,
            CameraPosition = Camera.transform.position,
            Door1State = Door1.isClosed,
        };
        return data;
    }

    public override void Load(SceneDataBaseClass d)
    {
        SceneData01_01 data = d as SceneData01_01;
        Player.transform.position = data.PlayerPosition;
        Camera.transform.position = data.CameraPosition;
        Door1.SetState(data.Door1State);
        Camera.UnseenAreas[Door1.UnseenAreaIndex].enable = true;
    }
}
