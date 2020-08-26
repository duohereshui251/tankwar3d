using ChobiAssets.KTP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    // 操控类型
    public enum CtrlType
    {
        none,
        player,
        computer
    }
    public CtrlType ctrlType = CtrlType.player;
    // 生命值
    private float maxHp = 100;
    public float hp = 100;
    // 子弹
    public GameObject bullet;
    public GameObject destoryEffect;
    // 上一次开跑时间
    public float lastShootTime = 0;
    // 开炮的时间间隔
    private float shootInterval = 0.5f;

    // 重心
    private Rigidbody rigi;
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


    void Start()
    {
        rigi = gameObject.GetComponent<Rigidbody>();
        rigi.centerOfMass = new Vector3(rigi.centerOfMass.x, -0.1f, rigi.centerOfMass.z);
        turret = transform.Find("turret");
        gun = turret.Find("gun");


        wheels = transform.Find("wheels");
        tracks = transform.Find("tracks");

    }

    // Update is called once per frame
    void Update()
    {
        // 修改重心

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
            if (axleInfos[1] != null && axleinfo == axleInfos[1])
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
        if (ctrlType != CtrlType.player)
            return;
        motor = MaxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        // 制动
        brakeTorque = 0;
        foreach (AxleInfo axleInfo in axleInfos)
        {
            // rpm 轮轴旋转速度
            if (axleInfo.leftWheel.rpm > 5 && motor < 0)
                brakeTorque = MaxBrakeTorque;
            else if (axleInfo.leftWheel.rpm < -5 && motor > 0)
                brakeTorque = MaxBrakeTorque;
            continue;
        }
        //turretRotTarget = Camera.main.transform.eulerAngles.y;
        //turretRollTarget = Camera.main.transform.eulerAngles.x;
        TargetSignPos();
        // 发射炮弹
        if (Input.GetMouseButton(0))
            Shoot();
    }
    public void WheelsRotation(WheelCollider collider)
    {
        if (wheels == null)
            return;
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        // 旋转每个轮子
        foreach (Transform wheel in wheels)
        {
            wheel.rotation = rotation;
        }
    }
    public void TrackMove()
    {
        if (tracks == null)
            return;
        float offset = 0;
        if (wheels.GetChild(0) != null)
        {
            offset = wheels.GetChild(0).localEulerAngles.x / 90f;
        }
        foreach (Transform track in tracks)
        {
            MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer>();
            if (mr == null)
            {
                continue;
            }
            Material mtl = mr.material;
            mtl.mainTextureOffset = new Vector2(0, offset);
        }


    }
    public void Shoot()
    {
        Debug.Log("shoot");

        if (Time.time - lastShootTime < shootInterval)
            return;
        if (bullet == null)
        {
            Debug.Log("bullet is none");
            return;
        }
        Vector3 pos = gun.position + gun.forward * 5;
        Instantiate(bullet, pos, gun.rotation);
        lastShootTime = Time.time;
        // 自己打自己
        //BeAttacked(30f);

    }
    public void BeAttacked(float att)
    {
        if (hp <= 0)
            return;
        if (hp > 0)
        {
            hp -= att;
        }
        if (hp <= 0)
        {
            GameObject destoryObj = (GameObject)Instantiate(destoryEffect);
            destoryObj.transform.SetParent(transform, false);
            destoryObj.transform.localPosition = new Vector3(0,0, 0.3f);
            ctrlType = CtrlType.none;

        }

    }
    public void TargetSignPos()
    {
        Vector3 hitPoint = Vector3.zero;
        RaycastHit raycastHit;

        Vector3 centerVec = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = Camera.main.ScreenPointToRay(centerVec);

        // 射线检测, 获取HitPoint
        if( Physics.Raycast(ray, out raycastHit, 400.0f))
        {
            hitPoint = raycastHit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(400);
        }
        Vector3 dir = hitPoint - turret.position;
        Quaternion angle = Quaternion.LookRotation(dir);
        turretRotTarget = angle.eulerAngles.y;
        turretRollTarget = angle.eulerAngles.x;
        Transform targetCube = GameObject.Find("TargetCube").transform;
        targetCube.position = hitPoint;
    }
}
