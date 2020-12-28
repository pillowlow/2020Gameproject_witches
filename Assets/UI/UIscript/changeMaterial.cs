using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class changeMaterial : MonoBehaviour
{
    public Material a,b,c,d,e;
    public float san;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        san = SanityValue.san;
        if(san > 60){
            GetComponent<Image>().material = e;
        }else if(san<=60 && san >= 40){
            GetComponent<Image>().material = a;
        }else if(san < 40 && san >= 20){
            GetComponent<Image>().material = b;
        }else if(san < 20 && san > 0){
            GetComponent<Image>().material = c;
        }else if(san == 0){
            GetComponent<Image>().material = d;
        } 
    }
}
