using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Transform : MonoBehaviour
{

    public ParticleSystem eff;
    
    void Start()
    {
        
    }


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P) && PlayerManager.sanityValue <= 60){

            if(PlayerManager.mode ==PlayerManager.ModeCode.normal)
                Transform(PlayerManager.ModeCode.transform);
            else if (PlayerManager.mode == PlayerManager.ModeCode.transform)
                Transform(PlayerManager.ModeCode.normal);
        }
    }

    public void Transform(PlayerManager.ModeCode targatMode)
    {
        if(PlayerManager.mode != targatMode)
        {
            Vector3 pos = transform.position;
            GameObject targetPlayer;

            if (PlayerManager.mode == PlayerManager.ModeCode.normal)
            {
                PlayerManager.mode = PlayerManager.ModeCode.transform;
                targetPlayer = PlayerManager.instance.player_transform;
            }
            else
            {
                PlayerManager.mode = PlayerManager.ModeCode.normal;
                targetPlayer = PlayerManager.instance.player;
            }

            targetPlayer.SetActive(true);
            targetPlayer.transform.position = pos;

            GameObject eff_ = Instantiate(eff.gameObject);
            eff_.gameObject.transform.position = pos;
            eff_.GetComponent<ParticleSystem>().Play();

            PlayerManager.state = PlayerManager.StateCode.idel;

            



            this.gameObject.SetActive(false);
        }
    }
}
