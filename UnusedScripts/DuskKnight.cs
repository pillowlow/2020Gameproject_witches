using DragonBones;
using UnityEngine;
using Animation = DragonBones.Animation;

public class DuskKnight : Enemy
{
    private Animation anim;
    public GameObject Shield;
    private Rigidbody2D rig;
    private string lastanim;
    private Vector2 PlayerPosition;
    private AttackMode mode;
    enum AttackMode
    {
        Idle,Closing
    }
    private void Start()
    {
        anim = GetComponent<UnityArmatureComponent>().animation;
        rig = GetComponent<Rigidbody2D>();
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(),Shield.GetComponent<Collider2D>());
    }

    private void Update()
    {
        if (state != StateCode.Die)
        {
            SearchPlayer();
            ChangeAnimation();
        }
    }
    
    
    private void SearchPlayer()
    {
        PlayerPosition = PlayerManager.instance.player.transform.position;
        float distance = Vector3.Distance(PlayerPosition, transform.position);
        if (distance<50 && distance>3)
        {
            mode = AttackMode.Closing;
            int facing = PlayerPosition.x - transform.position.x > 0 ? -1 : 1;
            ChangeFacing(facing);
            rig.velocity = new Vector2(-2 * facing, rig.velocity.y);
        }
        else
        {
            mode = AttackMode.Idle;
        }
        Debug.Log(mode);
    }

    private void AttackUI()
    {
        
    }

    private void ChangeFacing(int facing)
    {
        Vector3 theScale = transform.localScale;
        if (facing > 0 && theScale.x < 0 || facing < 0 && theScale.x > 0)
        {
            theScale.x *= -1;
            transform.localScale = theScale;
        }
        transform.localScale = theScale;
    }
    

    private void ChangeAnimation()
    {
        string animation;
        switch (mode)
        {
           case AttackMode.Idle:
               animation = "idle_shield";
               break;
           case AttackMode.Closing:
               animation = "walk_shield";
               break;
           default:
               animation = "idle_shield";
               break;
        }
        if (lastanim != animation)
        {
            anim.FadeIn(animation,0.2f);
            lastanim = animation;
        }
        
    }
    
}