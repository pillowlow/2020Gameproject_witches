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
    // Start is called before the first frame update
    void Start()
    {
        
        counter = 0;
        direction =1;
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (counter%(2*effectstrength)==effectstrength)
        {
            direction *= -1;
        }
        print(direction);
        container = speed * direction;
        transform.Translate(container,0,0);

        counter++;
    }
}
