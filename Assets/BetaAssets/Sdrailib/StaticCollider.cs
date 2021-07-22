using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticCollider : MonoBehaviour
{
    public LayerMask playerLayer;
    public float Opaque = 0.5f;
    public float FadingSpeed = 1.0f;
    private SpriteRenderer spriteRenderer;
    private float currentAlpha = 1;
    private bool notFading = true;
    private bool notFadingIn = true;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & playerLayer) != 0))
        {
            notFading = false;
            if(currentAlpha>Opaque)
            {
                currentAlpha = (currentAlpha >= Opaque) ? (currentAlpha - FadingSpeed * Time.deltaTime) : Opaque;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, currentAlpha);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & playerLayer) != 0))
        {
            notFading = true;
            if(notFadingIn)
            {
                StartCoroutine(nameof(FadeIn));
            }
        }
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
