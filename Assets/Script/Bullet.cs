using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 100f;
    public GameObject explode;
    public float maxLiftTime = 2f;
    public float instantiateTime = 0f;
    public GameObject attackTank;
    // Start is called before the first frame update
    void Start()
    {
        instantiateTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: 增加弹道
        Debug.Log("Bullet update");
        transform.position += transform.forward * speed * Time.deltaTime;
        if (Time.time  - instantiateTime > maxLiftTime)
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        Debug.Log("Bullet destroy");
        // 爆炸效果
        if (collisionInfo.gameObject == attackTank)
            return;

        Instantiate(explode, transform.position, transform.rotation);
        Destroy(gameObject);
        // 击中坦克
        Tank tank = collisionInfo.gameObject.GetComponent<Tank>();
        if(tank != null)
        {
            float att = GetAtt();
            tank.BeAttacked(att, attackTank);
        }
    }
    // 炮弹飞行时间越久，攻击力越小
    private float GetAtt()
    {
        float att = 100 - (Time.time - instantiateTime) * 40;
        if (att < 1)
            att = 1;
        return att;
    }
}
