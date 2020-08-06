using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class enemyDetect : MonoBehaviour
{
    // Start is called before the first frame update
    public LayerMask playerLayer;
    public float detectRange;
    private bool detectPlayer=false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        detectPlayer = Physics2D.OverlapCircle(gameObject.transform.position, detectRange,playerLayer);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(gameObject.transform.position, detectRange);
    }
}
