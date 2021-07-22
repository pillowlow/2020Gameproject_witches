using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace CustomEventNamespace
{
    [RequireComponent(typeof (BoxCollider2D))]
    public class CameraEventTrigger : MonoBehaviour,CustomEvent
    {
        [SerializeField] private string _showText = "";
    
        [SerializeField] private CinemachineVirtualCamera _camera = null;
    
        [SerializeField] public float hightLightTime { get; set; } = 3.0f;
    
        public CameraEventTrigger(CinemachineVirtualCamera camera,String text)
        {
            _camera = camera;
            _showText = text;
        }
        private void Awake()
        {
            _camera.enabled = false;
        }
    
        void OnTriggerEnter2D(Collider2D coll)
        {
            if(coll.CompareTag("Player")&&!string.IsNullOrEmpty(_showText))
            {
                PlayerManager.state = PlayerManager.StateCode.Stop;
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
            PlayerManager.state = PlayerManager.StateCode.Idle;
            Destroy(gameObject);
        }
        void ResetEventCamera()
        {
            _camera.enabled = false;
            Dialog.Instance.HideTextArea();
            PlayerManager.state = PlayerManager.StateCode.Idle;
        }
        
    
        public void StartEvent(OnInteract action)
        {
            PlayerManager.state = PlayerManager.StateCode.Stop;
            _camera.enabled = true;
            Invoke(nameof(ShowDialog), 2.0f);
            Invoke(nameof(ResetEventCamera),hightLightTime);
        }
    }

}
