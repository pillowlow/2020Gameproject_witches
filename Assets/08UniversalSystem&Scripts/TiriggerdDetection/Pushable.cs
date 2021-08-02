using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]

public class Pushable : MonoBehaviour
{
    public static bool isPushing = false;
    public static Transform PushedObject;
    public GameObject box;
    private Rigidbody2D boxRig;
    private BoxCollider2D boxCollider;
    public float distance = 0.1f;
    public LayerMask Wall;
    private bool freeToMove = true;
    public float pivot_height;
    private Vector2 box_size;

    private void Start()
    {
        boxRig = box.GetComponent<Rigidbody2D>();
        boxCollider = box.GetComponent<BoxCollider2D>();
        box_size = new Vector2(boxCollider.size.x * box.transform.lossyScale.x, boxCollider.size.y * box.transform.lossyScale.y - 0.1f);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0) && ((box.transform.position.x - PlayerManager.instance.player.transform.position.x) > 0 ? (PlayerManager.instance.input.GetKey(InputAction.Right)) : (PlayerManager.instance.input.GetKey(InputAction.Left))))
        {
            if(freeToMove)
            {
                if (PlayerManager.instance.input.GetKey(InputAction.Interact))
                {
                    isPushing = true;
                    //if (PlayerManager.state == PlayerManager.StateCode.Action_push)
                    {
                        if(CheckFall())
                        {
                            return;
                        }
                        else
                        {
                            boxRig.isKinematic = true;
                        }
                        box.transform.parent = PlayerManager.instance.player.transform;
                        PushedObject = box.transform;
                        StartCoroutine(nameof(StartPushing));
                        StartCoroutine(nameof(StopPushing));
                    }
                }
                else
                {
                    isPushing = false;
                }
            }
            else
            {
                boxCollider.enabled = false;
                Collider2D col = Physics2D.OverlapBox(box.transform.position, box_size, 0, Wall);
                if (col == null)
                {
                    freeToMove = true;
                }
                boxCollider.enabled = true;
            }
        }
    }

    IEnumerator StopPushing()
    {
        while(PlayerManager.instance.input.GetKey(InputAction.Interact) && isPushing)
        {
            Collider2D col = Physics2D.OverlapBox(box.transform.position, box_size, 0, Wall);
            if (col != null)
            {
                Release();
                freeToMove = false;
                yield return new WaitUntil(()=> {return PlayerManager.instance.input.GetKey(InputAction.Interact) && PlayerManager.state == PlayerManager.StateCode.Action_push; });
                isPushing = false;
                Release();
                yield break;
            }
            if (CheckFall()) { yield break; }
            yield return new WaitForEndOfFrame();
        }
        isPushing = false;
        Release();
    }
    IEnumerator StartPushing()
    {
        Vector3 Dest = new Vector3(box.transform.position.x + (PlayerMovement.instance.orient ? distance : -distance), box.transform.position.y, box.transform.position.z);
        boxCollider.enabled = false;
        while (isPushing && Mathf.Abs(Dest.x-transform.position.x) > 0.05f)
        {
            Vector3 Dir = Dest - box.transform.position;
            Collider2D col = Physics2D.OverlapBox(box.transform.position, box_size, 0, Wall);
            if (col != null)
            {
                Release();
                yield break;
            }
            box.transform.position += Dir * Time.deltaTime;
            if (CheckFall()) 
            {
                yield break; 
            }
            yield return new WaitForEndOfFrame();
        }
        if(Mathf.Abs(Dest.x - box.transform.position.x) <= 0.005f)
        {
            box.transform.position = Dest;
        }
    }

    void Release()
    {
        box.transform.parent = null;
        boxRig.velocity = new Vector2(0, 0);
        boxCollider.enabled = true;
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(box, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    bool CheckFall()
    {
        Vector2 Pivot = new Vector2(0, -pivot_height) + (Vector2)transform.position;
        Collider2D col = Physics2D.OverlapPoint(Pivot, Wall);
        if(col != null)
        {
            return false;
        }
        isPushing = false;
        Release();
        boxRig.isKinematic = false;
        return true;
    }
}
