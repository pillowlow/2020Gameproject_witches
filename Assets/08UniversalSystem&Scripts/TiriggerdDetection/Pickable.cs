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
    public bool isWeapon;
    [SerializeField] private float offset_time;
    [SerializeField] private float attack_time = 1;
    [SerializeField] private AnimationCurve attack_rotation;

    public static bool isTaking = false;
    public static Pickable held;
    private Transform OriginalParent;
    private Rigidbody2D rig;
    private Collider2D m_collider;
    private Vector2 Scale;
    private Transform Pivot;
    private void Start()
    {
        box = transform.parent.gameObject;
        rig = box.transform.parent.GetComponent<Rigidbody2D>();
        m_collider = box.GetComponent<Collider2D>();
        Pivot = box.transform.parent;
        Scale = Pivot.transform.localScale;
        OriginalParent = Pivot.transform.parent;
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
                m_collider.enabled = false;
                StartCoroutine(nameof(StartPicking));
            }
            if(PlayerManager.isTaking && held == this)
            {
                rig.simulated = false;
                Pivot.parent = hand;
                m_collider.enabled = false;
            }
        }
    }

    public void Throw(Vector2 force)
    {
        Pivot.parent = OriginalParent;
        rig.simulated = true;
        rig.isKinematic = false;
        m_collider.enabled = true;
        held = null;
        PlayerManager.instance.isFreeToDoAction = true;
        box.layer = 16;
        rig.AddForce(force);
        Pivot.transform.localScale = new Vector2((Pivot.transform.localScale.x > 0 ? Scale.x : -Scale.x), Scale.y);
    }

    IEnumerator StartPicking()
    {
        float time = 0;
        Vector2 ori_pos = Pivot.transform.position;
        Quaternion ori_rot = Pivot.transform.rotation;
        Vector2 final_position;
        Vector3 final_rot_euler = final_rotation.eulerAngles;
        Quaternion _final_rotation = Quaternion.Euler(final_rot_euler.x, final_rot_euler.y, PlayerMovement.instance.orient ? final_rot_euler.z : -final_rot_euler.z);
        while (time <= offset_time)
        {
            PlayerMovement.instance.SlowDown(32);
            final_position = new Vector2((PlayerMovement.instance.orient ? offset_position.x : -offset_position.x), offset_position.y) + (Vector2)PlayerManager.instance.player.transform.position;
            Vector2 cur_pos = Vector2.Lerp(ori_pos, final_position, time / offset_time);
            Quaternion cur_rot = Quaternion.Lerp(ori_rot, _final_rotation, time / offset_time);

            Pivot.transform.SetPositionAndRotation(cur_pos, cur_rot);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        final_position = new Vector2((PlayerMovement.instance.orient ? offset_position.x : -offset_position.x), offset_position.y) + (Vector2)PlayerManager.instance.player.transform.position;
        Pivot.transform.SetPositionAndRotation(final_position, _final_rotation);
        Pivot.transform.parent = hand;
    }

    public void PutDown()
    {
        Pivot.transform.parent = hand_put_down;
    }

    public void AttackRotation()
    {
        StartCoroutine(nameof(AttackRotationCoroutine));
    }
    IEnumerator AttackRotationCoroutine()
    {
        float time = 0;
        float ori_degree = Pivot.transform.rotation.eulerAngles.z;
        bool orient = Pivot.transform.lossyScale.x > 0;
        while(time < attack_time)
        {
            float eval = attack_rotation.Evaluate(time / attack_time);
            float value = ori_degree + (orient ? eval : -eval);
            Pivot.transform.rotation = Quaternion.Euler(box.transform.rotation.eulerAngles.x, box.transform.rotation.eulerAngles.y, value);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
