using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class InputManager
{
    private readonly InputConfig inputConfig;
    KeyCode[] Keys;
    public bool enable = true;
    float LastRightTap = -1;
    float LastLeftTap = -1;
    public InputManager()
    {
        if(inputConfig == null)
        {
            inputConfig = new InputConfig();
        }
        Keys = inputConfig.ToKeys();
        SaveSetting();
    }
    public bool GetKey(InputAction action)
    {
        return Input.GetKey(Keys[(int)action]) && enable;
    }

    public bool GetKeyDown(InputAction action)
    {
        return Input.GetKeyDown(Keys[(int)action]) && enable;
    }

    public bool GetKeyUp(InputAction action)
    {
        return Input.GetKeyUp(Keys[(int)action]) && enable;
    }

    public void BindKey(InputAction action, KeyCode key)
    {
        Keys[(int)action] = key;
        inputConfig.KeyCodes[action] = key;
    }

    public void ResetKey(InputAction action)
    {
        KeyCode key = inputConfig.Default[action];
        Keys[(int)action] = key;
        inputConfig.KeyCodes[action] = key;
    }

    public void ResetAllKeys()
    {
        inputConfig.KeyCodes = inputConfig.Default;
    }

    public float GetHorizonInput()
    {
        return Input.GetKey(Keys[(int)InputAction.Right]) && enable ? 1 : (Input.GetKey(Keys[(int)InputAction.Left]) && enable ? -1 : 0);
    }

    public float GetVerticalInput()
    {
        return Input.GetKey(Keys[(int)InputAction.Up]) && enable ? 1 : (Input.GetKey(Keys[(int)InputAction.Down]) && enable ? -1 : 0);
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

    public void TryLoadSetting()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "input.cfg");
        if(System.IO.File.Exists(path))
        {
            string content = System.IO.File.ReadAllText(path);
            string[] lines = content.Split('\n');
            foreach (var i in lines)
            {
                string[] key = i.Split(':');
                if (key.Length == 2)
                {
                    if (Enum.TryParse(key[0], out InputAction action))
                    {
                        inputConfig.KeyCodes[action] = (KeyCode)Enum.Parse(typeof(KeyCode), key[1]);
                    }
                }
            }
            Keys = inputConfig.ToKeys();
        }
    }

    public bool DoubleTapRight()
    {
        if(GetKeyDown(InputAction.Right) && enable)
        {
            float time = Time.time;
            bool result = time - LastRightTap < 1f;
            LastRightTap = time;
            return result;
        }
        return false;
    }

    public bool DoubleTapLeft()
    {
        if (GetKeyDown(InputAction.Left) && enable)
        {
            float time = Time.time;
            bool result = time - LastLeftTap < 1f;
            LastLeftTap = time;
            return result;
        }
        return false;
    }
    public bool Investigate(Collider2D target_collider)
    {
        if(Input.GetKey(KeyCode.Mouse0) && enable)
        {
            Vector2 mouse = CameraController.ScreenToWorldPoint(Input.mousePosition);
            if (target_collider == null) { return true; }
            return target_collider.OverlapPoint(mouse);
        }
        return false;
    }
}
