using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SanityValue : MonoBehaviour
{
    public bool inoil = false;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("check",0,1);
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.tag == "Player"){
            inoil = true;
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
       if(coll.tag == "Player"){
            inoil = false;
        }
    }

    void check()
    {
        if(PlayerManager.state != PlayerManager.StateCode.Stop){
            if(inoil && PlayerManager.sanityValue>=10) PlayerManager.AssignSanityValue(-10);
            else if(inoil && PlayerManager.sanityValue < 10) PlayerManager.sanityValue = 0;
            else if(PlayerManager.sanityValue < 100) PlayerManager.AssignSanityValue(1);
        }
    }
}

