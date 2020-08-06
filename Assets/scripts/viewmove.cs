using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class viewmove : MonoBehaviour
{
   private float lengthx, startposx, lengthy, startposy; 
       public GameObject cam;
       public float parllaxEffectX;
       public float parllaxEffectY;
       
       void Start()
       {
           startposx =transform.position.x;
           lengthx=GetComponent<SpriteRenderer>().bounds.size.x;
           startposy =transform.position.y;
           lengthy=GetComponent<SpriteRenderer>().bounds.size.y;
       }
   
       // Update is called once per frame
       void FixedUpdate()
       {
           
           float distx = (cam.transform.position.x * parllaxEffectX);
           float disty = (cam.transform.position.y * parllaxEffectY);
    
           transform.position=new Vector3(startposx+distx, y: startposy+disty,transform.position.z);
           
           
           
           
       }
}
