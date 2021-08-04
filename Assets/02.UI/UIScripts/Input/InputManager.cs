using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class InputManager
{
    InputConfig inputConfig;
    KeyCode[] Keys;
    public InputManager()
    {
        if(inputConfig==null)
        {
            inputConfig = new InputConfig();
        }
        Keys = inputConfig.ToKeys();
        SaveSetting();
    }
    public bool GetKey(InputAction action)
    {
        return Input.GetKey(Keys[(int)action]);
    }

    public bool GetKeyDown(InputAction action)
    {
        return Input.GetKeyDown(Keys[(int)action]);
    }

    public bool GetKeyUp(InputAction action)
    {
        return Input.GetKeyUp(Keys[(int)action]);
    }
    public void BindKey(InputAction action, KeyCode key)
    {
        Keys[(int)action] = key;
    }
    public bool isHorizonInput()
    {
        return Input.GetKey(Keys[(int)InputAction.Right]) || Input.GetKey(Keys[(int)InputAction.Left]);
    }

    public bool isVerticalInput()
    {
        return Input.GetKey(Keys[(int)InputAction.Up]) || Input.GetKey(Keys[(int)InputAction.Down]);
    }
    public void SaveSetting()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "input.cfg");
        string content = "";
        foreach(var i in inputConfig.KeyCodes)
        {
            content += i.Key.ToString() + ':' + i.Value.ToString() + '\n';
        }
        System.IO.File.WriteAllText(path, content);
    }
    public void LoadSetting()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "input.cfg");
        string content = System.IO.File.ReadAllText(path);
        string[] lines = content.Split('\n');
        foreach(var i in lines)
        {
            string[] key = i.Split(':');
            if(key.Length == 2)
            {
                InputAction action = (InputAction)Enum.Parse(typeof(InputAction),key[0]);
                inputConfig.KeyCodes[action] = (KeyCode)Enum.Parse(typeof(KeyCode), key[1]);
            }
        }
        Keys = inputConfig.ToKeys();
    }
}
