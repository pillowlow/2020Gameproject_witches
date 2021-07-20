using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "InputManager", menuName = "ScriptableObjects/InputManager")]
public class InputManager : ScriptableObject
{
    [SerializeField]
    KeyCode[] Keys = //Initial Values
    { 
        KeyCode.D,          //Right
        KeyCode.A,          //Left
        KeyCode.Space,      //Jump
        KeyCode.F,          //Interact
        KeyCode.E,          //Inventory
        KeyCode.LeftShift   //Sprint
    };

    public bool GetKey(InputAction action)
    {
        if(Input.GetKey(KeyCode.A))
        {
            Debug.Log(((int)action).ToString());
            Debug.Log(Keys[(int)action].ToString());
        }
       

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
