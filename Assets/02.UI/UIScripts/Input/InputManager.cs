using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "InputManager", menuName = "ScriptableObjects/InputManager")]
public class InputManager : ScriptableObject
{
    [SerializeField]
    KeyCode[] Keys = //Default Values
    { 
        KeyCode.D,          //Right
        KeyCode.A,          //Left
        KeyCode.Space,      //Jump
        KeyCode.S,          //Down
        KeyCode.F,          //Interact
        KeyCode.E,          //Inventory
        KeyCode.LeftShift   //Sprint
                            //...
    };

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

}
