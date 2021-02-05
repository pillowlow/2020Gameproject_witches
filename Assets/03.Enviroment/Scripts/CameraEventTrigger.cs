using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
[RequireComponent(typeof (BoxCollider2D))]
public class CameraEventTrigger : MonoBehaviour
{
    [SerializeField] private string _showText = "";

    [SerializeField] private CinemachineVirtualCamera _camera = null;

    [SerializeField] public float hightLightTime { get; set; } = 3.0f;
    private void Awake()
    {
        _camera.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.CompareTag("Player")&&!string.IsNullOrEmpty(_showText))
        {
            PlayerManager.moveable = false;
            _camera.enabled = true;
            Invoke(nameof(ShowDialog), 2.0f);
            Invoke(nameof(ResetCamera),hightLightTime);
        }
    }

    void ShowDialog()
    {
        Dialog.Instance.ShowTextArea(_showText);
    }
    void ResetCamera()
    {
        _camera.enabled = false;
        Dialog.Instance.HideTextArea();
        PlayerManager.moveable = true;
        Destroy(gameObject);
    }
    
}
