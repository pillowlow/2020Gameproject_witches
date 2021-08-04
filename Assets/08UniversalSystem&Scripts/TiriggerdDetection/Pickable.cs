using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable : MonoBehaviour
{
    [HideInInspector] public GameObject box;
    [SerializeField] private Transform hand;
    [SerializeField] private Transform hand_put_down;
    [SerializeField] private Vector2 offset_position;
    [SerializeField] private Quaternion final_rotation;
    [SerializeField] public bool isWeapon;
    [SerializeField] private float offset_time;
    [SerializeField] private float attack_time = 1;
    [SerializeField] private AnimationCurve attack_rotation;

    public static bool isTaking = false;
    public static Pickable held;
    private Transform OriginalParent;
    private Rigidbody2D rig;
    private Collider2D collider;
    private Vector2 Scale;
    private void Start()
    {
        box = transform.parent.gameObject;
        rig = box.GetComponent<Rigidbody2D>();
        collider = box.GetComponent<Collider2D>();
        Scale = box.transform.localScale;
        OriginalParent = box.transform.parent;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0))
        {
            if(PlayerManager.instance.input.GetKeyDown(InputAction.Interact) && PlayerManager.instance.isFreeToDoAction)
            {
                if (held != null) return;
                isTaking = true;
                rig.isKinematic = true;
                held = this;
                PlayerManager.instance.isFreeToDoAction = false;
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
        box.transform.parent = OriginalParent;
        rig.simulated = true;
        rig.isKinematic = false;
        collider.enabled = true;
        held = null;
        PlayerManager.instance.isFreeToDoAction = true;
        box.layer = 16;
        rig.AddForce(force);
        box.transform.localScale = new Vector2((box.transform.localScale.x > 0 ? Scale.x : -Scale.x), Scale.y);
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

            box.transform.SetPositionAndRotation(cur_pos, cur_rot);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        box.transform.SetPositionAndRotation(final_position, _final_rotation);
    }

    public void PutDown()
    {
        box.transform.parent = hand_put_down;
    }

    public void AttackRotation()
    {
        StartCoroutine(nameof(AttackRotationCoroutine));
    }
    IEnumerator AttackRotationCoroutine()
    {
        float time = 0;
        float ori_degree = box.transform.rotation.eulerAngles.z;
        bool orient = box.transform.lossyScale.x > 0;
        while(time < attack_time)
        {
            float eval = attack_rotation.Evaluate(time / attack_time);
            float value = ori_degree + (orient ? eval : -eval);
            box.transform.rotation = Quaternion.Euler(box.transform.rotation.eulerAngles.x, box.transform.rotation.eulerAngles.y, value);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
