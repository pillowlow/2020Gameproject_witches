using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    // 這個程式會附加到撥放器main Camera內，這裡死區deadZone設置為0，可以在unity內調到想要的效果
        private GameObject target;
    
        public float deadZone = 0;
        
        public float rightBound;
        public float leftBound;
        public float topBound;
        public float bottomBound;
    
       
        void Start()
        {
            //讓攝影機追蹤玩家
            if (target == null)
            {
                target = GameObject.Find("Player");
            }
        }
    
        void Update()
        {
            if (target != null)
            {
                //檢查移動的水平方向與死區，移動的水平Ｘ軸速度也要將死區數值加減近來
                
                if (transform.position.x >= target.transform.position.x + deadZone)
                {
                    transform.position = new Vector3(Mathf.Clamp(target.transform.position.x + deadZone,leftBound,rightBound), transform.position.y,transform.position.z);
                }
                if (transform.position.x <= target.transform.position.x - deadZone)
                {
                    transform.position = new Vector3(Mathf.Clamp(target.transform.position.x - deadZone,leftBound,rightBound), transform.position.y, transform.position.z); 
                }
                
    
                
                if (transform.position.y >= target.transform.position.y + deadZone)
                {
                    transform.position = new Vector3(transform.position.x, Mathf.Clamp(target.transform.position.y + deadZone, bottomBound,topBound), transform.position.z);
                        
                }
                if (transform.position.y <= target.transform.position.y - deadZone)
                {
                    transform.position = new Vector3(transform.position.x, Mathf.Clamp(target.transform.position.y - deadZone, bottomBound, topBound), transform.position.z);
                }
                
            }
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawLine(new Vector2(leftBound-12,topBound+5),new Vector2(leftBound-12,bottomBound-5) );
            Gizmos.DrawLine(new Vector2(rightBound+12,topBound+5),new Vector2(rightBound+12,bottomBound-5));
            Gizmos.DrawLine(new Vector2(leftBound-12,topBound+5),new Vector2(rightBound+12,topBound+5) );
            Gizmos.DrawLine(new Vector2(leftBound-12,bottomBound-5),new Vector2(rightBound+12,bottomBound-5) );
        }
}
