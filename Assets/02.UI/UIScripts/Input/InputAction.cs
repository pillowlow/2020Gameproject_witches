using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputAction
{
    Right,
    Left,
    Up,
    Jump,
    Down,
    Interact,
    Attack,
    Number_of_Actions
}

public class InputConfig
{
    public readonly Dictionary<InputAction, KeyCode> Default = new Dictionary<InputAction, KeyCode>
    {
        {InputAction.Right, KeyCode.D },
        {InputAction.Left, KeyCode.A },
        {InputAction.Up, KeyCode.W },
        {InputAction.Jump, KeyCode.Space },
        {InputAction.Down, KeyCode.S },
        {InputAction.Interact, KeyCode.E },
        {InputAction.Attack, KeyCode.K }
    };
    public Dictionary<InputAction, KeyCode> KeyCodes;
    public InputConfig()
    {
        KeyCodes = Default;
    }
    public KeyCode[] ToKeys()
    {
        KeyCode[] keys = new KeyCode[(int)InputAction.Number_of_Actions];
        foreach(var i in KeyCodes)
        {
            keys[(int)i.Key] = i.Value;
        }
        return keys;
    }
}