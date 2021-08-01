using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LightControll : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.Light2D editLight;
    
    public int flashLength = 1;
    public int normalLength = 5;
    
    
    public float flashfrequent = 2;
    public float flashstrength = 1;
    public int cycleOffest = 2;
    bool isFlash = false;
    float timer;
    float oriBright;
    float cyclecount;
    int x,y = 0;
    int i,j = 0;
    
    

    
    

    // Start is called before the first frame update
    void Start()
    {
        oriBright = editLight.intensity;
        
        x = UnityEngine.Random.Range(0,10);
        

    }

    // Update is called once per frame
    void Update()
    {
        y = UnityEngine.Random.Range(-cycleOffest,cycleOffest);
        j++;
        if(j % flashfrequent == 0)
            i++;
        timer = Mathf.Floor(Time.time);
        cyclecount = (timer + x) % (flashLength+normalLength+y);
        

        if(cyclecount < flashLength)
            isFlash = true;
        else
            isFlash = false;

        if(isFlash)
            editLight.intensity = i%2*oriBright;
        else
            editLight.intensity = oriBright;
    }
        
}
