using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticCollider : MonoBehaviour
{
    [SerializeField] private float Opaque = 0.5f;
    [SerializeField] private float FadingSpeed = 1.0f;
    [SerializeField] private float ColliderInvalidTime = 0.5f;
    [SerializeField] private float AdditionalGravity = 1.0f;
    private SpriteRenderer spriteRenderer;
    private float currentAlpha = 1;
    private bool notFading = true;
    private bool notFadingIn = true;
    private Collider2D m_collider;
    private Rigidbody2D Character;
    [HideInInspector] public static bool isNotDropping = true;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        m_collider = GetComponent<Collider2D>();
        Character = PlayerMovement.instance.rig;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if ((((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0))
        {
            Vector2 bottom = PlayerManager.instance.player.transform.position + new Vector3(0, -1.86f);
            if(m_collider.OverlapPoint(bottom))
            {
                if (PlayerManager.instance.input.GetKeyDown(InputAction.Down))
                {
                    m_collider.isTrigger = true;
                    isNotDropping = false;
                    PlayerMovement.instance.rig.gravityScale += AdditionalGravity;
                }
            }
            else
            {
                Stay();
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0))
        {
            Stay();
        }    
    }
    private void Stay()
    {
        notFading = false;
        if (currentAlpha > Opaque)
        {
            currentAlpha = (currentAlpha >= Opaque) ? (currentAlpha - FadingSpeed * Time.deltaTime) : Opaque;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, currentAlpha);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0))
        {
            Exit();
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if ((((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0))
        {
            Exit();
        }
    }

    private void Exit()
    {
        notFading = true;
        if (notFadingIn)
        {
            StartCoroutine(nameof(FadeIn));
        }
        m_collider.isTrigger = false;
        PlayerMovement.instance.rig.gravityScale = 1;
        isNotDropping = true;
    }

    IEnumerator FadeIn()
    {
        notFadingIn = false;
        while (currentAlpha < 1 && notFading)
        {
            currentAlpha = (currentAlpha <= 1.0f) ? (currentAlpha + FadingSpeed * Time.deltaTime) : 1.0f;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, currentAlpha);
            yield return new WaitForEndOfFrame();
        }
        notFadingIn = true;
    }
}
