using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    // 炮塔
    public Transform turret;
    private float turretRotSpeed = 0.5f;
    private float turretRotTarget = 0;
    private float turretRollTarget = 0;

    // 炮管
    public Transform gun;
    private float maxRoll = 10f;
    private float minRoll = -4f;


    // 移动速度
    public float speed = 5f;
    public float steer = 50;

    // Start is called before the first frame update
    void Start()
    {
        turret = transform.Find("turret");
        gun = transform.Find("gun");
    }

    // Update is called once per frame
    void Update()
    {
        // 旋转
        float x = Input.GetAxis("Horizontal");
        transform.Rotate(0, x * steer * Time.deltaTime, 0);

        // 前进后退
        float y = Input.GetAxis("Vertical");
        Vector3 s = y * transform.forward * speed * Time.deltaTime;
        transform.transform.position += s;
        // 炮塔角度
        turretRotTarget = Camera.main.transform.eulerAngles.y;
        turretRollTarget = Camera.main.transform.eulerAngles.x;
        TurretRotation();
    }

    public void TurretRotation()
    {
        if (Camera.main == null)
            return;
        if (turret == null)
            return;
        float angle = turret.eulerAngles.y - turretRotTarget;
        // 如果目标角度在左边
        if (angle < 0) angle += 360;

        if (angle > turretRotSpeed && angle < 180)
        {
            // 逆时针
            turret.Rotate(0f, -turretRotSpeed, 0f);
        } else if (angle > 180 && angle < 360 - turretRotSpeed)
        {
            //顺时针
            turret.Rotate(0f, turretRotSpeed, 0f);
        }
    }
    public void TurretRoll()
    {
        if (Camera.main == null)
            return;
        if (turret == null)
            return;
        Vector3 worldEuler = gun.eulerAngles;
        Vector3 localEuler = gun.localEulerAngles;


    }
}
