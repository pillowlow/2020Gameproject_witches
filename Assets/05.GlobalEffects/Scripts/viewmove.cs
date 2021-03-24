using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class viewmove : MonoBehaviour
{
   private float  camstartposx, camstartposy,startposx,startposy; 
       public GameObject cam;
       public float parllaxEffectX;
       public float parllaxEffectY;
       public bool bgornot;

       private bool _iscamNull;
       //private float startassignpostX;
      //private float startassignpostY;
       
       void Start()
       {
           _iscamNull = cam == null;
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
            if(_iscamNull)
                return;
            var temp= ParllaxCount(cam.transform.position.x,cam.transform.position.y,bgornot:bgornot);
            transform.position=new Vector3(startposx+temp.Item1, y: startposy+temp.Item2,transform.position.z);

       }

        (float,float) ParllaxCount(float camx, float camy, bool bgornot)
        {
            float distx, disty;
            distx = ((cam.transform.position.x-0) * parllaxEffectX);
            disty = ((cam.transform.position.y-0) * parllaxEffectY);
            if (bgornot == true)
            {
                transform.position = new Vector3(cam.transform.position.x, y: cam.transform.position.y, transform.position.z);
            }
            return (distx, disty);
            
        }
        
}
