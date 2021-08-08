using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject OpenedDoor;
    public GameObject ClosedDoor;
    public GameObject UI;
    public GameObject UnlockUI;
    public GameObject OpenUI;
    public bool isLocked = false;
    public bool isClosed = true;
    public int UnseenAreaIndex = 0;
    private InputManager input;
    private Renderer shader;
    private static bool Yes = false;
    private static bool No = false;
    private bool UnlockOrOpen = true;

    private void Start()
    {
        input = PlayerManager.instance.input;
    }
    
    bool StopWaiting = false;
    
    
    public void QueryToUnlockDoor(OnInteract interact)
    {
        UI.SetActive(true);
        UnlockOrOpen = true;
        StartCoroutine(WaitForRespone(interact));
    }

    public void QueryToOpenDoor(OnInteract interact)
    {
        UI.SetActive(true);
        UnlockOrOpen = false;
        StartCoroutine(WaitForRespone(interact));
    }

    public void SetState(bool s)
    {
        if(s)
        {
            CloseDoor(null);
        }
        else
        {
            OpenDoor(null);
        }
        StartCoroutine(WaitForRespone(null));
    }
    
    public void OpenDoor([CanBeNull]OnInteract interact)
    {
        OpenedDoor.SetActive(true);
        ClosedDoor.SetActive(false);
        isClosed = false;
        if(UnseenAreaIndex>=0)
        {
            CameraController.instance.UnseenAreas[UnseenAreaIndex].enable = false;
        }
    }

    private void CloseDoor([CanBeNull]OnInteract interact)
    {
        OpenedDoor.SetActive(false);
        ClosedDoor.SetActive(true);
        isClosed = true;
        CameraController.instance.UnseenAreas[UnseenAreaIndex].enable = false;
        if(interact != null) interact.SetEventDone(transform);
    }

    IEnumerator WaitForRespone([CanBeNull]OnInteract interact)
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
                UnlockUI.SetActive(false);
                OpenDoor(interact);
                CameraController.instance.StartCameraMovement(0);
            }
        }
        else
        {
            if (Yes)
            {
                OpenUI.SetActive(false);
                OpenDoor(interact);
            }
        }
        Yes = false;
        No = false;
        UI.SetActive(false);
        if(isClosed) interact.SetEventDone(false);
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
