using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SanityValue : MonoBehaviour
{
    public Material san_mat;
    static public float san;
    public Image im_san;
    public bool inoil = false;
    // Start is called before the first frame update
    void Start()
    {
        san = 99;
        InvokeRepeating("check",0,1);
    }

    // Update is called once per frame
    void Update()
    {
        im_san.fillAmount = san/100.0f;
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
        if(inoil && san>=10) san -= 10;
        else if(inoil && san < 10) san = 0;
        else if(san < 100)san++;
    }
}

