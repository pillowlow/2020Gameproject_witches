using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance = null;
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
        public bool enable = true;
    };
    [System.Serializable]
    public class CameraMovement
    {
        public GameObject Begin;
        public GameObject End;
        public AnimationCurve Displacement;
        public float Speed;
    }
    public UnseenArea[] UnseenAreas;
    public CameraMovement[] CameraMovements;
    public float CameraMovingSpeed = 4;

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
    private float WaitingTime = 0;

    private float zoom;
    private bool HasMoved = false;
    private bool FreeToZoom = true;
    public bool isCameraMovement { get; private set; } = false;
    private int cameraMovementIndex = 0;
    private Rigidbody2D Character;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            zoom = Farest;
            foreach (var i in cam)
            {
                i.orthographicSize = zoom;
            }
            Character = PlayerManager.instance.player.GetComponent<Rigidbody2D>();
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
    }
    float time = 0;
    void Update()
    {
        if (PlayerManager.instance.player != null)
        {
            if(isCameraMovement)
            {
                time += Time.deltaTime;
                float ratio = CameraMovements[cameraMovementIndex].Displacement.Evaluate(time * CameraMovements[cameraMovementIndex].Speed);
                if (ratio == 1)
                {
                    isCameraMovement = false;
                }
                Vector2 newPos = Vector2.Lerp(CameraMovements[cameraMovementIndex].Begin.transform.position, CameraMovements[cameraMovementIndex].End.transform.position, ratio);
                transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
                return;
            }

            FreeToZoom = true;
            foreach(var i in TriggerZone)
            {
                float dx = i.target.transform.position.x - PlayerManager.instance.player.transform.position.x;
                float dy = i.target.transform.position.y - PlayerManager.instance.player.transform.position.y;
                float d = dx * dx + dy * dy;
                if(d < i.radius*i.radius)
                {
                    Zoom(i.zoom, i.speed, i.factor);
                    FreeToZoom = false;
                    break;
                }
            }

            if (FreeToZoom)
            {
                if (Mathf.Abs(Character.velocity.x) > 0.5 || Mathf.Abs(Character.velocity.y) > 0.5)
                {
                    HasMoved = true;
                }
                if (HasMoved)
                {
                    if (!Zoom(Farest, ZoomOutSpeed, 1))
                    {
                        zoom = Farest;
                        HasMoved = false;
                    }
                    WaitingTime = 0;
                }
                else if ((WaitingTime += Time.deltaTime) > WaitTime)
                {
                    Zoom(Closest,ZoomInSpeed,SpeedFactor);
                }
            }

            MoveCameraTo(PlayerManager.instance.transform.position);
        }
    }

    public bool Zoom( float z, float speed, float factor)
    {
        float d = (z - zoom) * Time.deltaTime * speed;
        if (d < 0.00001f && d > -0.00001f)
        {
            return false;
        }
        foreach (var i in cam)
        {
            i.orthographicSize = (zoom += d);
        }
        return true;
    }
    void MoveCameraTo(Vector2 target)
    {
        Vector2 dir = new Vector2(target.x - transform.position.x, target.y - transform.position.y) * Time.deltaTime * CameraMovingSpeed;
        Vector2 nextPosition = new Vector2(transform.position.x + dir.x, transform.position.y + dir.y);
        float VerticalRadius = zoom;
        float HorizontalRadius = zoom * Screen.width / Screen.height;
        float right = nextPosition.x + HorizontalRadius;
        float left = nextPosition.x - HorizontalRadius;
        float up = nextPosition.y + VerticalRadius;
        float down = nextPosition.y - VerticalRadius;

        float ori_right = transform.position.x + HorizontalRadius;
        float ori_left = transform.position.x - HorizontalRadius;
        float ori_up = transform.position.y + VerticalRadius;
        float ori_down = transform.position.y - VerticalRadius;

        bool xFree = true;
        bool yFree = true;
        float newX = nextPosition.x;
        float newY = nextPosition.y;
        foreach (var i in UnseenAreas)
        {
            if(i.enable)
            {
                float r_right = i.BottomRight.transform.position.x;
                float r_left = i.UpperLeft.transform.position.x;
                float r_up = i.UpperLeft.transform.position.y;
                float r_down = i.BottomRight.transform.position.y;

                bool r_in_middle = (right > r_left && right < r_right);
                bool u_in_middle = (up > r_down && up < r_up);

                float x_min = r_in_middle ? (right - r_left) : (r_right - left);
                float y_min = u_in_middle ? (up - r_down) : (r_up - down);

                if (x_min > y_min)
                {
                    if (yFree && (((ori_right > r_left && ori_right < r_right) || ((ori_left > r_left && ori_left < r_right) || (ori_left < r_left && ori_right > r_right)))
                    && (u_in_middle || (down > r_down && down < r_up) || (up > r_up && down < r_down))))
                    {
                        yFree = false;
                        newY = (up > r_up) ? r_up + VerticalRadius : r_down - VerticalRadius;
                    }
                }
                else
                {
                    if (xFree && ((r_in_middle || ((left > r_left && left < r_right) || (left < r_left && right > r_right)))
                && ((ori_up > r_down && ori_up < r_up) || (ori_down > r_down && ori_down < r_up) || (ori_up > r_up && ori_down < r_down))))
                    {
                        xFree = false;
                        newX = (left < r_left) ? r_left - HorizontalRadius : r_right + HorizontalRadius;
                    }
                }

                if (!(xFree || yFree))
                {
                    break;
                }
            }
        }
        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    public void StartCameraMovement(int index)
    {
        isCameraMovement = true;
        cameraMovementIndex = index;
        time = 0;
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
