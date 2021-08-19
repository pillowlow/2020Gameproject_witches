using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfTransparent : MonoBehaviour
{
    [SerializeField] private float Opaque = 0.5f;
    [SerializeField] private float FadingSpeed = 1.0f;
    [SerializeField] private SpriteRenderer Sprites;
    private float currentAlpha = 1;
    private bool notFading = true;
    private bool notFadingIn = true;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0))
        {
            notFading = false;
            if (currentAlpha > Opaque)
            {
                currentAlpha = (currentAlpha >= Opaque) ? (currentAlpha - FadingSpeed * Time.deltaTime) : Opaque;
                Sprites.color = new Color(Sprites.color.r, Sprites.color.g, Sprites.color.b, currentAlpha);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0))
        {
            notFading = true;
            if (notFadingIn)
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
            Sprites.color = new Color(Sprites.color.r, Sprites.color.g, Sprites.color.b, currentAlpha);
            yield return new WaitForEndOfFrame();
        }
        notFadingIn = true;
    }
}
