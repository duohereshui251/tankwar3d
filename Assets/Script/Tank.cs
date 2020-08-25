using ChobiAssets.KTP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    // 履带
    private Transform tracks;
    // 轮子
    private Transform wheels;
    // 炮塔
    public Transform turret;
    private float turretRotSpeed = 0.5f;
    private float turretRotTarget = 0;
    private float turretRollTarget = 0;

    // 炮管
    public Transform gun;
    private float maxRoll = 10f;
    private float minRoll = -4f;

    // 轮轴的马力、制动、转角控制
    public List<AxleInfo> axleInfos;
    // 马力
    private float motor = 0;
    public float MaxMotorTorque;
    // 制动 
    private float brakeTorque = 0;
    public float MaxBrakeTorque = 100;
    // 转向角
    private float steering = 0;
    public float maxSteeringAngle;

    //// 移动速度
    //public float speed = 5f;
    //public float steer = 50;

    // Start is called before the first frame update
    void Start()
    {
        turret = transform.Find("turret");
        gun = turret.Find("gun");
        Rigidbody rigi = gameObject.GetComponent<Rigidbody>();
        //Vector3 l = transform.localPosition;
        //rigi.centerOfMass = new Vector3(l.x, l.y ,l.z);
        wheels = transform.Find("wheels");
        tracks = transform.Find("tracks");
    }

    // Update is called once per frame
    void Update()
    {
        //  玩家控制操控
        PlayerCtrl();

        foreach (AxleInfo axleinfo in axleInfos)
        {
            // 控制转向
            if (axleinfo.steering)
            {
                axleinfo.leftWheel.steerAngle = steering;
                axleinfo.rightWheel.steerAngle = steering;
            }
            // 马力
            if (axleinfo.motor)
            {
                axleinfo.leftWheel.motorTorque = motor;
                axleinfo.rightWheel.motorTorque = motor;
            }
            // 制动
            if (true)
            {
                axleinfo.leftWheel.brakeTorque = brakeTorque;
                axleinfo.rightWheel.brakeTorque = brakeTorque;
            }
            if(axleInfos[1] != null && axleinfo == axleInfos[1])
            {
                WheelsRotation(axleInfos[1].leftWheel);
                TrackMove();
            }

        }
        //// 旋转
        //float x = Input.GetAxis("Horizontal");
        //transform.Rotate(0, x * steer * Time.deltaTime, 0);

        //// 前进后退
        //float y = Input.GetAxis("Vertical");
        //Vector3 s = y * transform.forward * speed * Time.deltaTime;
        //transform.transform.position += s;
        //// 炮塔角度
        //turretRotTarget = Camera.main.transform.eulerAngles.y;
        //turretRollTarget = Camera.main.transform.eulerAngles.x;
        //// 炮筒角度

        // 炮塔炮管旋转
        TurretRotation();
        TurretRoll();

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
        }
        else if (angle > 180 && angle < 360 - turretRotSpeed)
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

        // 相对于自身旋转的欧拉角
        Vector3 worldEuler = gun.eulerAngles;
        // 相对于父对象的欧拉角
        Vector3 localEuler = gun.localEulerAngles;

        // 先自转
        worldEuler.x = turretRollTarget;
        gun.eulerAngles = worldEuler;

        // 如果超过相对父对象的角度，则修正
        Vector3 euler = gun.localEulerAngles;
        if (euler.x > 180)
            euler.x -= 360;

        if (euler.x > maxRoll)
            euler.x = maxRoll;
        if (euler.x < minRoll)
            euler.x = minRoll;
        gun.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);

    }
    public void PlayerCtrl()
    {
        motor = MaxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        // 制动
        brakeTorque = 0;
        foreach(AxleInfo axleInfo in axleInfos)
        {
            // rpm 轮轴旋转速度
            if (axleInfo.leftWheel.rpm > 5 && motor < 0)
                brakeTorque = MaxBrakeTorque;
            else if (axleInfo.leftWheel.rpm < -5 && motor > 0)
                brakeTorque = MaxBrakeTorque;
            continue;
        }
        turretRotTarget = Camera.main.transform.eulerAngles.y;
        turretRollTarget = Camera.main.transform.eulerAngles.x;
    }
    public void WheelsRotation(WheelCollider collider)
    {
        if (wheels == null)
            return;
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        // 旋转每个轮子
        foreach(Transform wheel in wheels)
        {
            wheel.rotation = rotation;
        }
    }
    public void TrackMove()
    {
        if (tracks == null)
            return;
        float offset = 0;
        if(wheels.GetChild(0) != null)
        {
            offset = wheels.GetChild(0).localEulerAngles.x / 90f;
        }
        foreach(Transform track in tracks)
        {
            MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer>();
            if(mr == null)
            {
                continue;
            }
            Material mtl = mr.material;
            mtl.mainTextureOffset = new Vector2(0, offset);
        }
            
                
    }
}
