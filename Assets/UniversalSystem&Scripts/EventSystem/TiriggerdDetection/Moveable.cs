using System;
using System.Collections;
using UnityEngine;

public class Moveable : MonoBehaviour
{
    public static bool ready2move = false;
    public static Moveable moved;
    [SerializeField] private float push_distance;
    [SerializeField] private float pull_distance;
    private GameObject box;
    private Collider2D m_collider;
    [HideInInspector] public Rigidbody2D rig;
    [HideInInspector] public RelativeJoint2D joint;
    private void Start()
    {
        box = transform.parent.gameObject;
        rig = box.GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0)
        {
            if(PlayerManager.instance.input.Investigate(m_collider) && PlayerManager.instance.isFreeToDoAction && PlayerManager.instance.input.GetHorizonInput() != 0)
            {
                if(moved == null)
                {
                    if(joint == null)
                    {
                        joint = box.AddComponent<RelativeJoint2D>();
                        joint.enableCollision = true;
                        joint.maxTorque = 0;
                    }
                    joint.enabled = true;
                    moved = this;
                    PlayerManager.instance.isFreeToDoAction = false;
                    ready2move = true;
                }
                else if(moved == this)
                {
                    ready2move = true;
                    PlayerManager.instance.isFreeToDoAction = false;
                    joint.maxForce = 1000;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0))
        {
            if(joint != null)
            {
                joint.enabled = false;
            }
            ready2move = false;
            moved = null;
        }
    }

    float distance;
    public void Start2Move(bool push)
    {
        distance = push ? push_distance : pull_distance;
        StartCoroutine(nameof(StartMoving));
    }
    IEnumerator StartMoving()
    {
        Vector3 Dest = new Vector3(box.transform.position.x + (PlayerMovement.instance.orient ? -distance : distance), PlayerManager.instance.player.transform.position.y, PlayerManager.instance.player.transform.position.z);
        while (ready2move && Mathf.Abs(Dest.x - PlayerManager.instance.player.transform.position.x) > 0.05f)
        {
            Dest = new Vector3(box.transform.position.x + (PlayerMovement.instance.orient ? -distance : distance), PlayerManager.instance.player.transform.position.y, PlayerManager.instance.player.transform.position.z);
            Vector3 Dir = Dest - PlayerManager.instance.player.transform.position;
            PlayerManager.instance.player.transform.position += Dir * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        if (Mathf.Abs(Dest.x - PlayerManager.instance.player.transform.position.x) <= 0.005f)
        {
            PlayerManager.instance.player.transform.position = Dest;
        }
    }
}
