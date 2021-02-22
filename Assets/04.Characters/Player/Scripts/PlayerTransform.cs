using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTransform : MonoBehaviour
{

    public ParticleSystem eff;
    

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P)){
            if(PlayerManager.sanityValue <= 60 && PlayerManager.mode == PlayerManager.ModeCode.normal){
                Transform(PlayerManager.ModeCode.transform);
            }else if (PlayerManager.mode == PlayerManager.ModeCode.transform)
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

            PlayerManager.state = PlayerManager.StateCode.idle;
            gameObject.SetActive(false);
            PlayerManager.instance.OnTransformFinish(targetPlayer);
        }
    }
}
