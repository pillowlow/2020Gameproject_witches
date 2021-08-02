using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable : MonoBehaviour
{
    public GameObject box;
    public Transform hand;
    public Vector2 offset_position;
    public Quaternion final_rotation;
    public bool isWeapon;
    public float offset_time;
    public static bool isTaking = false;
    public static Pickable held;
    private Rigidbody2D rig;
    private Collider2D collider;
    private void Start()
    {
        rig = box.GetComponent<Rigidbody2D>();
        collider = box.GetComponent<Collider2D>();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0))
        {
            if(PlayerManager.instance.input.GetKeyDown(InputAction.Interact))
            {
                if (held != null) return;
                isTaking = true;
                rig.isKinematic = true;
                held = this;
                box.layer = 12;
                StartCoroutine(nameof(StartPicking));
            }
            if(PlayerManager.isTaking && held == this)
            {
                rig.simulated = false;
                box.transform.parent = hand;
                collider.enabled = false;
            }
        }
    }

    public void Throw(Vector2 force)
    {
        box.transform.parent = null;
        rig.simulated = true;
        rig.isKinematic = false;
        collider.enabled = true;
        held = null;
        box.layer = 16;
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(box, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        rig.AddForce(force);
    }

    IEnumerator StartPicking()
    {
        float time = 0;
        Vector2 ori_pos = box.transform.position;
        Quaternion ori_rot = box.transform.rotation;
        Vector2 final_position = new Vector2((PlayerMovement.instance.orient ? offset_position.x : -offset_position.x), offset_position.y) + (Vector2)PlayerManager.instance.player.transform.position;
        Vector3 final_rot_euler = final_rotation.eulerAngles;
        Quaternion _final_rotation = Quaternion.Euler(final_rot_euler.x, final_rot_euler.y, PlayerMovement.instance.orient ? final_rot_euler.z : -final_rot_euler.z);
        while (time <= offset_time)
        {
            Vector2 cur_pos = Vector2.Lerp(ori_pos, final_position, time / offset_time);
            Quaternion cur_rot = Quaternion.Lerp(ori_rot, _final_rotation, time / offset_time);

            box.transform.position = cur_pos;
            box.transform.rotation = cur_rot;
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        box.transform.position = final_position;
        box.transform.rotation = _final_rotation;
    }

}
