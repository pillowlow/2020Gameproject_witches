using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyObject : MonoBehaviour
{
    [SerializeField] private byte disposable;//0 : is disposable and is disabled. 1 : is disposable and is enabled. 2 : is not disposable and is disabled 3 : is not disposable and is disabled.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (System.Convert.ToBoolean(disposable & 1) && ((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0)
        {
            PlayerMovement.instance.Killed();
            if(System.Convert.ToBoolean(disposable & 2))
            {
                disposable = 0;
            }
        }
    }
}
