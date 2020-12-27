using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Transform : MonoBehaviour
{
    public GameObject targetPlayer;
    public ParticleSystem eff;
    
    void Start()
    {
        
    }


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P)){
            Vector3 pos = transform.position;

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
