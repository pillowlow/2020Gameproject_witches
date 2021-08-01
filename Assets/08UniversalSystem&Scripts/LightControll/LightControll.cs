using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LightControll : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.Light2D editLight;
    
    public float flashStrength = 1;
    public float flashingSpeed = 30;
    public float flashingcycle = 100;
    public float flashinglength = 20;
    float timer;
    float Brightness ;
    int counter = 0;
    bool isFlash = false;

    // Start is called before the first frame update
    void Start()
    {
        Brightness = editLight.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        timer = Mathf.Floor(Time.time);
        if(timer % flashingcycle == 0　&& timer / flashingcycle != 0){
            isFlash = true;
        }


        if (isFlash) {
            editLight.intensity += Mathf.Sin(timer*flashingSpeed)*flashStrength;
            counter++;
        }


        
        if(isFlash && counter == flashinglength )
            isFlash = false;
            counter = 0;


        
        
        Debug.Log( isFlash);
        Debug.Log(counter);
        
    }
}
