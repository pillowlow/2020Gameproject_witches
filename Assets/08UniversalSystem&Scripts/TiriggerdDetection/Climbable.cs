using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbable : MonoBehaviour
{
    private BoxCollider2D m_collider;
    public static Climbable climbed = null;
    [HideInInspector] public Climbable Up = null;
    [HideInInspector] public Climbable Down = null;
    [HideInInspector] public Rigidbody2D rig;
    [HideInInspector] public Transform Root;
    private HingeJoint2D hinge;
    public float length {get; private set;}
    public float length_2 { get; private set; }
    private void Start()
    {
        m_collider = GetComponent<BoxCollider2D>();
        rig = GetComponent<Rigidbody2D>();
        hinge = GetComponent<HingeJoint2D>();
        GameObject Object_Up = hinge.connectedBody.gameObject;
        length_2 = m_collider.size.y * transform.lossyScale.y;
        length = length_2 * 2;
        if (Object_Up != null)
        {
            Up = Object_Up.GetComponent<Climbable>();
            if (Up != null)
            {
                Up.Down = this;
            }
        }
        else
        {
            Root = transform;
            Climbable current = Down;
            while (current != null)
            {
                Down.Root = Root;
                current = current.Down;
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0 )
        {
            if (PlayerManager.instance.input.GetKey(InputAction.Up) && m_collider.IsTouching(PlayerManager.instance.HandRange) && PlayerManager.instance.isFreeToDoAction)
            {
                if (climbed == null || transform.position.y > climbed.transform.position.y)
                {
                    climbed = this;
                    PlayerManager.instance.isFreeToDoAction = false;
                    PlayerMovement.instance.Climb();
                }
            }
        }
    }

    public Vector2 GetBottom()
    {
        float degree = transform.rotation.eulerAngles.z;
        float x = Mathf.Cos(Mathf.Deg2Rad * degree) * length_2;
        float y = Mathf.Sin(Mathf.Deg2Rad * degree) * length_2;
        return new Vector2(transform.position.x + x, transform.position.y + y);
    }
    public Vector2 GetTop()
    {
        if (Up != null)
        {
            return Up.GetBottom();
        }
        float degree = transform.rotation.eulerAngles.z + 180.0f;
        float x = Mathf.Cos(Mathf.Deg2Rad * degree) * length_2;
        float y = Mathf.Sin(Mathf.Deg2Rad * degree) * length_2;
        return new Vector2(transform.position.x + x, transform.position.y + y);
    }
}
