using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 100f;
    public GameObject explode;
    public float maxLiftTime = 2f;
    public float instantiateTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        instantiateTime = Time.time;
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        if (Time.time  - instantiateTime > maxLiftTime)
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collectionInfo)
    {
        
        //Instantiate(explode, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
