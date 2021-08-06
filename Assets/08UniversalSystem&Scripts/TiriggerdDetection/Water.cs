using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public static Vector2 Detect_Offset;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0)
        {
            PlayerManager.instance.isInWater = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0)
        {
            PlayerManager.instance.isInWater = false;
        }
    }
}
