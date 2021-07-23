using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject OpenedDoor;
    public GameObject ClosedDoor;
    public GameObject UI;
    public GameObject UnlockUI;
    public GameObject OpenUI;
    public GameObject Hint;
    public bool isLocked = false;
    public bool isClosed = true;
    public LayerMask playerLayer;
    public int UnseenAreaIndex = 0;
    private InputManager input;
    private Renderer shader;
    private static bool Yes = false;
    private static bool No = false;
    private bool UnlockOrOpen = true;
    private int shaderID;
    private Material mat;
    public float maxEffectStrength = 0.04f;

    private void OnEnable()
    {
        shader = ClosedDoor.GetComponent<Renderer>();
        mat = shader.material;
        shaderID = Shader.PropertyToID("Vector1_4C8E13CA");
        mat.SetFloat(shaderID, 0);
    }
    private void Start()
    {
        input = PlayerManager.instance.input;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isClosed && (((1 << collision.gameObject.layer) & playerLayer) != 0))
        {
            mat.SetFloat(shaderID, maxEffectStrength);
            Hint.SetActive(true);
            StopWaiting = false;
        }
    }
    bool StopWaiting = false;
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            mat.SetFloat(shaderID, 0);
            Hint.SetActive(false);
            UI.SetActive(false);
            StopWaiting = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            if (input.GetKeyDown(InputAction.Interact) && isClosed)
            {
                if (isLocked)
                {
                    //Check whether player has the key
                    if (true)
                    {
                        QueryToUnlockDoor();
                    }
                }
                else
                {
                    QueryToOpenDoor();
                }
            }
        }
    }

    private void QueryToUnlockDoor()
    {
        UI.SetActive(true);
        UnlockOrOpen = true;
        StartCoroutine(nameof(WaitForRespone));
    }

    private void QueryToOpenDoor()
    {
        UI.SetActive(true);
        UnlockOrOpen = false;
        StartCoroutine(nameof(WaitForRespone));
    }

    public void OpenDoor()
    {
        OpenedDoor.SetActive(true);
        ClosedDoor.SetActive(false);
        isClosed = false;
        Hint.SetActive(false);
        CameraController.instance.UnseenAreas[UnseenAreaIndex].enable = false;
    }

    IEnumerator WaitForRespone()
    {
        if (UnlockOrOpen)
        {
            UnlockUI.SetActive(true);
            OpenUI.SetActive(false);
        }
        else
        {
            UnlockUI.SetActive(false);
            OpenUI.SetActive(true);
        }

        yield return new WaitUntil(() => { return Yes || No|| StopWaiting; });
        if(StopWaiting)
        {
            yield break;
        }
        if(UnlockOrOpen)
        {
            if(Yes)
            {
                isLocked = false;
                UnlockUI.SetActive(false);
                OpenDoor();
                CameraController.instance.StartCameraMovement(0);
            }
        }
        else
        {
            if (Yes)
            {
                OpenUI.SetActive(false);
                OpenDoor();
            }
        }
        Yes = false;
        No = false;
        UI.SetActive(false);
    }


    public void YesButton()
    {
        Yes = true;
    }

    public void NoButton()
    {
        No = true;
    }
}
