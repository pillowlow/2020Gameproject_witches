using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrowmove : MonoBehaviour
{
    public float effectstrength;
    private float startposx, startposy;
    private int counter;
    private bool direction;
    // Start is called before the first frame update
    void Start()
    {
        startposx = transform.position.x;
        startposy = transform.position.y;
        counter = 0;
        direction = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (counter < effectstrength && counter > 0 && direction == true)
        {
            transform.position = new Vector3(startposx + counter, y: startposy + counter, transform.position.z);
            counter++;
        }
        else if (counter == effectstrength)
        {
            direction = false;
        }
        else if (counter < effectstrength && counter > 0 && direction == false)
        {
            transform.position = new Vector3(startposx + counter, y: startposy + counter, transform.position.z);
            counter--;
        }
    }
}
