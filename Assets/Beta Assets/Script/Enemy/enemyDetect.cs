using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class enemyDetect : MonoBehaviour
{
    // Start is called before the first frame update
    public LayerMask playerLayer;
    public bool box;
    public Vector2 detectRange;
    private bool detectPlayer=false;
    public Vector2 target;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void detectCircle()
    {
        detectPlayer = Physics2D.OverlapCircle(gameObject.transform.position, detectRange.x,playerLayer);
        if (detectPlayer)
        {
            GetComponent<AIPath>().canSearch = true;
        }
        else
        {
            GetComponent<AIPath>().canSearch = false;
        }

        if (gameObject.name == "Eagle Shoot")
        {
            if (detectPlayer)
            {
                GetComponent<enemyShoot>().enabled = true;
            }
            else
            {
                GetComponent<enemyShoot>().enabled = false;
            }
        }
    }

    private void detectBox()
    {
        //detectPlayer=Physics2D.OverlapBox(target,de)
    }
    private void OnDrawGizmosSelected()
    {
        if (box)
        {
            Gizmos.DrawCube(target,detectRange);
           
        }
        else
        {
            Gizmos.DrawWireSphere(target, detectRange.x);
        }
    }
}
