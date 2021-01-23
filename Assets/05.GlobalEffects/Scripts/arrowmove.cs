using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrowmove : MonoBehaviour
{
    public double effectstrength = 10;
    private float container;
    private int counter;
    private int direction;
    [SerializeField]private float speed ;
    private float startposx, startposy;
    
    // Start is called before the first frame update
    void Start()
    {
        startposx = transform.position.x;
        startposy = transform.position.y;
        counter = 0;
        direction =1;
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if ((counter*1000)%(2*(effectstrength*1000))==10*effectstrength)
        {
            direction *= -1;
        }
        print(direction);
        container = speed * direction;
        //transform.Translate(container,0,0);
        transform.position = new Vector3(x:startposx + container, y:transform.position.y, z :transform.position.z);
        counter++;
    }
}
