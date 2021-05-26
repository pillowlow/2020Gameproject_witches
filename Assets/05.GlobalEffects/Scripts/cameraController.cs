using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    // 這個程式會附加到撥放器main Camera內，這裡死區deadZone設置為0，可以在unity內調到想要的效果
    [System.Serializable]
    class TriggerZoneT
    {
        public GameObject target;
        public float zoom = 4.0f;
        public float speed = 1.0f;
        public float factor = 1.0f;
        public float radius = 1.0f;
    }
    public GameObject target;
    
    public float deadZone = 0;
    
    public float rightBound;
    public float leftBound;
    public float topBound;
    public float bottomBound;
    public Camera[] cam;
    [SerializeField]
    private TriggerZoneT[] TriggerZone;
    [Header("Zoom Config")]
    public float Closest = 2.0f;
    public float Farest = 4.0f;
    public float ZoomOutSpeed = 1.0f;
    public float ZoomInSpeed = 0.0005f;
    public float SpeedFactor = 0.7f;
    public float WaitTime = 2.0f;

    private float zoom;
    private float stopt;
    private float last;
    private bool HasMoved = false;
    private bool Free = true;
    private void Start()
    {
        zoom = Farest;
        last = Time.time;
        foreach (var i in cam)
        {
            i.orthographicSize = zoom;
        }
    }

    void Update()
    {
        if (target != null)
        {
            //檢查移動的水平方向與死區，移動的水平Ｘ軸速度也要將死區數值加減近來
            
            if (transform.position.x >= target.transform.position.x + deadZone)
            {
                transform.position = new Vector3(Mathf.Clamp(target.transform.position.x + deadZone,leftBound,rightBound), transform.position.y,transform.position.z);
            }
            if (transform.position.x <= target.transform.position.x - deadZone)
            {
                transform.position = new Vector3(Mathf.Clamp(target.transform.position.x - deadZone,leftBound,rightBound), transform.position.y, transform.position.z); 
            }
            
    
            
            if (transform.position.y >= target.transform.position.y + deadZone)
            {
                transform.position = new Vector3(transform.position.x, Mathf.Clamp(target.transform.position.y + deadZone, bottomBound,topBound), transform.position.z);
                    
            }
            if (transform.position.y <= target.transform.position.y - deadZone)
            {
                transform.position = new Vector3(transform.position.x, Mathf.Clamp(target.transform.position.y - deadZone, bottomBound, topBound), transform.position.z);
            }

            Free = true;
            foreach(var i in TriggerZone)
            {
                float dx = i.target.transform.position.x - target.transform.position.x;
                float dy = i.target.transform.position.y - target.transform.position.y;
                float d = dx * dx + dy * dy;
                if(d < i.radius*i.radius)
                {
                    Zoom(i.zoom,i.speed,i.factor);
                    Free = false;
                    break;
                }
            }

            if (Free)
            {
                if (Mathf.Abs(target.GetComponent<Rigidbody2D>().velocity.x) > 0.5 || Mathf.Abs(target.GetComponent<Rigidbody2D>().velocity.y) > 0.5)
                {
                    HasMoved = true;
                }
                if (HasMoved)
                {
                    if (!Zoom(Farest,ZoomOutSpeed,1))
                    { zoom = Farest; HasMoved = false; }
                    stopt = Time.time;
                }
                else if (Time.time - stopt > WaitTime)
                {
                    Zoom(Closest,ZoomInSpeed,SpeedFactor);
                }
            }
        }
        last = Time.time;
    }

    public bool Zoom( float z, float speed, float factor)
    {
        float d =  zoom - z;
        float duration = Time.time - last;
        if(d>0.005f)
        {
            zoom -= Mathf.Exp( d * factor) * speed * duration;
        }
        else if(d<-0.005f)
        {
            zoom += Mathf.Exp(-d * factor) * speed * duration;
        }
        else
        {
            zoom -= d;
            return false;
        }
        foreach (var i in cam)
        {
            i.orthographicSize = zoom;
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(new Vector2(leftBound-12,topBound+5),new Vector2(leftBound-12,bottomBound-5) );
        Gizmos.DrawLine(new Vector2(rightBound+12,topBound+5),new Vector2(rightBound+12,bottomBound-5));
        Gizmos.DrawLine(new Vector2(leftBound-12,topBound+5),new Vector2(rightBound+12,topBound+5) );
        Gizmos.DrawLine(new Vector2(leftBound-12,bottomBound-5),new Vector2(rightBound+12,bottomBound-5) );
    }
}
