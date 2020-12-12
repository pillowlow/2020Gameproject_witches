using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowRange : MonoBehaviour
{
    public float range;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(range, 0, 0));
    }
}
