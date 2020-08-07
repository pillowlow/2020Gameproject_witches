using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public float speed;

    private Transform player;
    private Vector2 target;
	
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        player = GameObject.Find("Player").transform;
        target=new Vector2(player.position.x,(player.position.y+1.25f));
        
		Vector2 lookDir = target - rb.position;
        float angle = Mathf.Atan2(lookDir.y,lookDir.x)*Mathf.Rad2Deg - 90f;
		rb.rotation=angle;
    }

    // Update is called once per frame
    void Update()
    {
		
		transform.Translate(Vector2.up * Time.deltaTime*speed);
       
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag=="Player")
        {
            if(!other.GetComponent<playerCombat>().isInvincible)
            {
                other.GetComponent<playerCombat>().TakeDamage(1);
                Destroy(gameObject);
            }
            
            
        }

        if (other.gameObject.tag == "Ground")
        {
            Destroy(gameObject);
        }
        
    }
}
