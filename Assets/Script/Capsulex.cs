using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capsulex : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Rigidbody rigi = gameObject.GetComponent<Rigidbody>();
            Vector3 force = Vector3.up * 50;
            // 向上添加力
            rigi.AddForce(force);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("碰撞到 " + collision.gameObject.name);
    }
}
