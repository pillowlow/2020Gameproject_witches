using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class EagleDetect : MonoBehaviour
{
    private AIPath AiPath;

    public LayerMask playerLayer;
    public float detectRange;
    private bool detectPlayer;

    private void Start()
    {
        AiPath = GetComponent<AIPath>();
    }

    private void Update()
    {
        DetectCircle();
        FlipAround();
    }

    private void DetectCircle()
    {
        detectPlayer = Physics2D.OverlapCircle(gameObject.transform.position, detectRange,playerLayer);
        if (detectPlayer)
        {
            AiPath.canSearch = true;
        }
        else
        {
            AiPath.canSearch = false;
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
        Gizmos.DrawWireSphere(gameObject.transform.position, detectRange);
    }
}
