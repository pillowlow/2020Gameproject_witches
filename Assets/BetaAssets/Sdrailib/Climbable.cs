using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbable : MonoBehaviour
{
    private BoxCollider2D collider;
    public static Climbable climbed = null;
    public Climbable Up = null;
    public Climbable Down = null;
    public Rigidbody2D rig;
    private void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        rig = GetComponent<Rigidbody2D>();
        HingeJoint2D hinge = GetComponent<HingeJoint2D>();
        GameObject Object_Up = hinge.connectedBody.gameObject;
        if (Object_Up != null)
        {
            Up = Object_Up.GetComponent<Climbable>();
            if (Up != null)
            {
                Up.Down = this;
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (climbed == null && ((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0 )
        {
            if (PlayerManager.instance.input.GetKeyDown(InputAction.Interact) && collider.IsTouching(PlayerManager.instance.ClimbRange))
            {
                climbed = this;
                PlayerMovement.instance.Climb();
            }
        }
    }
}
