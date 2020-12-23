using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogAttack : MonoBehaviour
{
    private Rigidbody2D Rigidbody2D;
    public GameObject player;

    public float maxJumpDistance;
    public float jumpForce;
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void jumpAttack()
    {
        float distanceFromPlayer = player.transform.position.x - gameObject.transform.position.x;
        Rigidbody2D.velocity=new Vector2(Mathf.Clamp(distanceFromPlayer, -maxJumpDistance, maxJumpDistance),jumpForce);
    }
}
