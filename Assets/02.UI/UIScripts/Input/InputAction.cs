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
    Crawl,
    Interact,
    Inventory,
    Sprint,
    Attack,
    Number_of_Actions
}

public class InputConfig
{
    Dictionary<InputAction, KeyCode> Default = new Dictionary<InputAction, KeyCode>
    {
        {InputAction.Right, KeyCode.D },
        {InputAction.Left, KeyCode.A },
        {InputAction.Up, KeyCode.W },
        {InputAction.Jump, KeyCode.Space },
        {InputAction.Down, KeyCode.S },
        {InputAction.Crawl, KeyCode.Z },
        {InputAction.Interact, KeyCode.F },
        {InputAction.Inventory, KeyCode.E },
        {InputAction.Sprint, KeyCode.LeftShift },
        {InputAction.Attack, KeyCode.K }
    };
    public Dictionary<InputAction, KeyCode> KeyCodes;
    public InputConfig()
    {
        Reset();
    }
    public void Reset()
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