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

    /// <summary>
    /// Enable or disable the input.
    /// </summary>
    /// <param name="b">True to disable.False to enable.</param>
    public void StopInput(bool b)
    {
        enable = !b;
    }

    /// <summary>
    /// Returns true while the corresponding key to the action is held down.
    /// </summary>
    /// <param name="action">The input action to ask.</param>
    public bool GetKey(InputAction action)
    {
        return Input.GetKey(Keys[(int)action]) && enable;
    }
    /// <summary>
    /// Returns true if it is the first frame that the player starts to press down the corresponding key to the action.
    /// </summary>
    /// <param name="action">The input action to ask.</param>
    public bool GetKeyDown(InputAction action)
    {
        return Input.GetKeyDown(Keys[(int)action]) && enable;
    }


    /// <summary>
    /// Returns true if it is the first frame that the player releases the corresponding key to the action.
    /// </summary>
    /// <param name="action">The input action to ask.</param>
    public bool GetKeyUp(InputAction action)
    {
        return Input.GetKeyUp(Keys[(int)action]) && enable;
    }

    /// <summary>
    /// Bind a key to an input action.
    /// </summary>
    /// <param name="action">The input action to bind.</param>
    /// <param name="key">The key code to bind.</param>
    public void BindKey(InputAction action, KeyCode key)
    {
        Keys[(int)action] = key;
        inputConfig.KeyCodes[action] = key;
    }

    /// <summary>
    /// Reset the binding of the input action.
    /// </summary>
    /// <param name="action">The input action to be reset.</param>
    public void ResetKey(InputAction action)
    {
        KeyCode key = inputConfig.Default[action];
        Keys[(int)action] = key;
        inputConfig.KeyCodes[action] = key;
    }

    /// <summary>
    /// Reset the bindings of every input action.
    /// </summary>
    public void ResetAllKeys()
    {
        inputConfig.KeyCodes = inputConfig.Default;
    }

    /// <summary>
    /// Get horizontal axis input.
    /// </summary>
    /// <returns>
    /// Returns horizontal axis input ranging from -1 to 1.
    /// </returns>
    public float GetHorizonInput()
    {
        return Input.GetKey(Keys[(int)InputAction.Right]) && enable ? 1 : (Input.GetKey(Keys[(int)InputAction.Left]) && enable ? -1 : 0);
    }


    /// <summary>
    /// Get Vertical axis input.
    /// </summary>
    /// <returns>
    /// Returns Vertical axis input ranging from -1 to 1.
    /// </returns>
    public float GetVerticalInput()
    {
        return Input.GetKey(Keys[(int)InputAction.Up]) && enable ? 1 : (Input.GetKey(Keys[(int)InputAction.Down]) && enable ? -1 : 0);
    }

    /// <summary>
    /// Save the configuration of the current input action binding to "input.cfg".
    /// </summary>
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

    /// <summary>
    /// Try to load the input action configuration from "input.cfg". 
    /// </summary>
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

    /// <summary>
    /// Returns true if the player double tapps the right key.
    /// </summary>
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

    /// <summary>
    /// Returns true if the player double tapps the left key.
    /// </summary>
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

    /// <summary>
    /// Returns true if the player left clicks on the target.
    /// </summary>
    /// <param name="target_collider">The collider of the target.</param>
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

    /// <summary>
    /// Returns the corresponding key code of the input action.
    /// </summary>
    /// <param name="action">The input action to ask.</param>
    public KeyCode GetKeyCode(InputAction action)
    {
        return Keys[(int)action];
    }
}
