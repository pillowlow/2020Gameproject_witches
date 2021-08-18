using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class light_yee : MonoBehaviour
{
    [SerializeField] GameObject light;
    [SerializeField] int DieTime;
    Coroutine Combustion;
    PlayerMovement playerMovement;
    bool Spilled;

    void OnTriggerEnter2D(Collider2D Player)
    {
        Debug.Log(8);
        if(Player.gameObject.layer != 12) { return; }
        Debug.Log(9);
        if(playerMovement == null)
        {
            playerMovement = Player.gameObject.GetComponent<PlayerMovement>();
        }
        //first went by
        if(!Spilled) 
        {
            Debug.Log(10);
            Spilled = true;
            int direction = playerMovement.rig.velocity.x > 0 ? 1 : 0; //if animation fall left
            transform.rotation = Quaternion.Euler(0, 180 * direction, 0);
            GetComponent<Animator>().SetTrigger("Spill");
            return;
        }
        Debug.Log(0);
        Combustion = StartCoroutine(Combust());
    }
    void OnTriggerExit2D(Collider2D Player)
    {
        if(Player.gameObject.layer != 12) { return; }
        if(Combustion == null) { return; }
        Debug.Log(2);
        StopCoroutine(Combustion);
    }
    //stay too long will die
    IEnumerator Combust()
    {
        yield return new WaitForSeconds(DieTime);
        Debug.Log(1);
        playerMovement.Killed();
        yield break;
    }
}
