using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class viewmove : MonoBehaviour
{
   private float  camstartposx, camstartposy,startposx,startposy; 
       public GameObject cam;
       public float parllaxEffectX;
       public float parllaxEffectY;
       
       void Start()
       {    
           if(cam == null)
               return;
           camstartposx = cam.transform.position.x;
           camstartposy = cam.transform.position.y;
           startposx = transform.position.x;
           startposy = transform.position.y;
       }
   
       // Update is called once per frame
        void Update()
        { 
            if(cam == null)
                return;
            var temp= ParllaxCount(cam.transform.position.x,cam.transform.position.y);
            transform.position=new Vector3(startposx+temp.Item1, y: startposy+temp.Item2,transform.position.z);

       }

        (float,float) ParllaxCount(float camx, float camy)
        {
            float distx, disty;
            distx = ((cam.transform.position.x-camstartposx) * parllaxEffectX);
            disty = ((cam.transform.position.y-camstartposy) * parllaxEffectY);
            return (distx, disty);
        }
}
