using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FwClosingChange : MonoBehaviour
{
    public GameObject cam;
    public float Xdgreepercent;
    public float Ydgreepercent;
    private float Xbound;
    private float Ybound;    
    private float Xdistance;
    private float Ydistance;
    // Start is called before the first frame update
    void Start()
    {
    Xbound = GetComponent<SpriteRenderer>().bounds.size.x;
    Ybound = GetComponent<SpriteRenderer>().bounds.size.y;
}

    // Update is called once per frame
    void Update()
    {
        if (PositionCheck(cam))
        {
            
        }
    }

    bool PositionCheck(GameObject cam)
    {
        Xdistance = (cam.transform.position.x - transform.position.x);
        Ydistance = (cam.transform.position.x - transform.position.x);
        if(Xdistance <=  Xbound*(1+0.01*Xdgreepercent) && Ydistance <=  Ybound*(1+0.01*Ydgreepercent))
        {
            return true;
        }

        return false;
    }
}
