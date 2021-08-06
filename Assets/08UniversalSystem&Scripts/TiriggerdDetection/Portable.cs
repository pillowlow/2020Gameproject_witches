using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portable : MonoBehaviour
{
    public static Portable ported;
    public static bool ready2port = false;
    [SerializeField] private float Animation_time = 2;
    [SerializeField] public float Putdown_time = 1;
    private Transform OriginalParent;
    [SerializeField] private AnimationCurve Pickup_Rotation_offset;
    [SerializeField] private AnimationCurve Pickup_y_offset;
    [SerializeField] private AnimationCurve Pickup_x_offset;
    [SerializeField] private float x_offset = 1;
    [HideInInspector] public GameObject box;
    private Rigidbody2D rig;
    private void Start()
    {
        box = transform.parent.gameObject;
        OriginalParent = box.transform.parent;
        rig = box.GetComponent<Rigidbody2D>();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & PlayerManager.instance.layer) != 0)
        {
            if(PlayerManager.instance.input.GetKey(InputAction.Interact) && ported == null && PlayerManager.instance.isFreeToDoAction)
            {
                ported = this;
                PlayerManager.instance.isFreeToDoAction = false;
                ready2port = true;
                StartCoroutine(nameof(Take));
            }
        }
    }

    IEnumerator Take()
    {
        float time = 0;
        rig.simulated = false;
        box.GetComponent<Collider2D>().enabled = false;
        Vector3 ori_degree = box.transform.rotation.eulerAngles;
        float ori_pos = box.transform.position.x;
        while (time<Animation_time)
        {
            time += Time.deltaTime;
            float ratio = time / Animation_time;
            float x_final = Mathf.Lerp(ori_pos, box.transform.position.x + (PlayerMovement.instance.orient ? x_offset : -x_offset), Pickup_x_offset.Evaluate(ratio));
            box.transform.SetPositionAndRotation(new Vector3(x_final, box.transform.position.y + Pickup_y_offset.Evaluate(ratio), box.transform.position.z), Quaternion.Euler(ori_degree.x, ori_degree.y, ori_degree.z + (PlayerMovement.instance.orient ? Pickup_Rotation_offset.Evaluate(ratio) : -Pickup_Rotation_offset.Evaluate(ratio))));
            yield return new WaitForEndOfFrame();
        }
        box.transform.parent = PlayerManager.instance.RightHand;
    }
    public void PutDown()
    {
        box.transform.parent = OriginalParent;
        ported = null;
        PlayerManager.instance.isFreeToDoAction = true;
        ready2port = false;
        rig.simulated = true;
        box.GetComponent<Collider2D>().enabled = true;
    }

}
