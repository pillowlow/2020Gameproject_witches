using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyShoot : MonoBehaviour
{
    public GameObject bulletA;
    public GameObject player;
    
    private float ShootingTime ;
    public float ShootingNeedTime ;

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.x < player.transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        AIShooting();
    }
    void AIShooting()
    {
        if (ShootingTime <= 0)  //ShootingTime <= 0
        {
            ShootingTime = ShootingNeedTime;  //設定ShootingTime為ShootingNeedTime
            GameObject bullet = (GameObject)Instantiate(bulletA, transform.position, new Quaternion(0, 0, 0, 0));
            //克隆一個Bullet在小飛兵的位置，轉型成GameObject型態將值給予bullet
        }
        else //否則ShootingTime減去Time.deltaTime達成計時效果
        {
            ShootingTime -= Time.deltaTime;
        }

        
    }
}
