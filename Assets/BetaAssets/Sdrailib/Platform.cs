using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public LayerMask playerLayer;
    public float ColliderInvalidTime = 0.5f;
    public float AdditionalGravity = 2;
    private InputManager input;
    private BoxCollider2D Coliider;
    private Rigidbody2D Character;
    // Start is called before the first frame update
    void Start()
    {
        input = PlayerManager.instance.input;
        Character = PlayerManager.instance.GetComponent<Rigidbody2D>();
        Coliider = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if ((((1 << collision.gameObject.layer) & playerLayer) != 0))
        {
            if(input.GetKeyDown(InputAction.Down))
            {
                StartCoroutine(nameof(FallDownPlatform));
            }
        }
    }

    IEnumerator FallDownPlatform()
    {
        Coliider.enabled = false;
        Character.gravityScale += AdditionalGravity;
        yield return new WaitForSeconds(ColliderInvalidTime);
        Coliider.enabled = true;
        Character.gravityScale -= AdditionalGravity;
    }
}
