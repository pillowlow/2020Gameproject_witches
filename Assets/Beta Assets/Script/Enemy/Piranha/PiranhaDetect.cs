using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PiranhaDetect : MonoBehaviour
{
    private AIPath AiPath;
    private PiranhaPatrol PiranhaPatrol;
    
    public GameObject target;
    public LayerMask playerLayer;
    public Vector2 detectRange;
    private bool detectPlayer=false;// Start is called before the first frame update

    private void Start()
    {
        AiPath = GetComponent<AIPath>();
        PiranhaPatrol = GetComponent<PiranhaPatrol>();
    }

    private void Update()
    {
        detectBox();
        FlipAround();
    }

    private void detectBox()
    {
        detectPlayer = Physics2D.OverlapBox(target.transform.position, detectRange, playerLayer);
        if (detectPlayer)
        {
            AiPath.canSearch = true;
            PiranhaPatrol.enabled = false;

        }
        else
        {
            AiPath.canSearch = false;
            PiranhaPatrol.enabled = true;
        }
    }

    private void FlipAround()
    {
        if (AiPath.desiredVelocity.x >= 0.01f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (AiPath.desiredVelocity.x <= -0.01f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(target.transform.position, detectRange);
    }
}
