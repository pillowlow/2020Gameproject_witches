using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
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
    };
    [System.Serializable]
    public class UnseenArea
    {
        public GameObject UpperLeft;
        public GameObject BottomRight;
    };
    public GameObject target;
    public UnseenArea[] UnseenAreas;
    

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
    private Rigidbody2D Character;
    private void Start()
    {
        zoom = Farest;
        last = Time.time;
        foreach (var i in cam)
        {
            i.orthographicSize = zoom;
        }
        Character = target.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (target != null)
        {
            //檢查移動的水平方向與死區，移動的水平Ｘ軸速度也要將死區數值加減近來

            transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);

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
                if (Mathf.Abs(Character.velocity.x) > 0.5 || Mathf.Abs(Character.velocity.y) > 0.5)
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
        if(d>0.005f)
        {
            zoom -= Mathf.Exp( d * factor) * speed * Time.deltaTime;
        }
        else if(d<-0.005f)
        {
            zoom += Mathf.Exp(-d * factor) * speed * Time.deltaTime;
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

    private void OnDrawGizmos()
    {
        foreach(var a in UnseenAreas)
        {
            if (a.BottomRight != null && a.UpperLeft != null)
            {
                Gizmos.DrawWireCube(new Vector3((a.UpperLeft.transform.position.x + a.BottomRight.transform.position.x) / 2, (a.UpperLeft.transform.position.y + a.BottomRight.transform.position.y) / 2, 0.1f),
                            new Vector3((a.BottomRight.transform.position.x - a.UpperLeft.transform.position.x), (a.UpperLeft.transform.position.y - a.BottomRight.transform.position.y), 0.1f));

            }
        }
    }
}
