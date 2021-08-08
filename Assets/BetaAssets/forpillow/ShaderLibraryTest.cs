using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnyPortrait;

public class ShaderLibraryTest : MonoBehaviour
{
    public apPortrait portrait;
    public float fadeValue = 0.5f;
    void Update ()
       {
            if ( Input.GetKey(KeyCode.W) )
            {
                 fadeValue -= Time.deltaTime * 0.5f;
                 portrait.SetMeshCustomFloatAll(fadeValue, "_DissolveValue");
                Debug.Log("tests");
            }
            if ( Input.GetKey(KeyCode.Q) )
            {
                 fadeValue += Time.deltaTime * 0.5f;
                 portrait.SetMeshCustomFloatAll(fadeValue, "_DissolveValue");
            }
           
       }
}
