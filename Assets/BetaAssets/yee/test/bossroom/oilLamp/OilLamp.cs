using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilLamp : MonoBehaviour
{
    [SerializeField] GameObject light;
    [SerializeField] int DieTime;
    Coroutine Combustion;
    PlayerMovement playerMovement;
    bool Spilled;

    void OnTriggerEnter2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12) { return; }
        if(playerMovement == null)
        {
            playerMovement = Player.gameObject.GetComponent<PlayerMovement>();
        }
        //first went by
        if(!Spilled) 
        {
            Spilled = true;
            /*
            if(playerMovement.rig.velocity.x > 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0)
            }
            */
            int direction = playerMovement.rig.velocity.x > 0 ? 1 : 0; //if animation fall left
            transform.rotation = Quaternion.Euler(0, 180 * direction, 0);
            GetComponent<Animator>().SetTrigger("Spill");
            return;
        }
        Combustion = StartCoroutine(Combust());
    }
    void OnTriggerExit2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12) { return; }
        if(Combustion == null) { return; }
        StopCoroutine(Combustion);
    }
    //stay too long will die
    IEnumerator Combust()
    {
        yield return new WaitForSeconds(DieTime);
        playerMovement.Killed();
        yield break;
    }
}
