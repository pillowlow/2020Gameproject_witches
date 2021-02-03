using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _virtualCamera = null;
    // Start is called before the first frame update

    private void Start()
    {
        GameObject instancePlayer = PlayerManager.instance.player;
        if (_virtualCamera != null)
        {
            _virtualCamera.Follow = instancePlayer.transform;
        }
    }
}
