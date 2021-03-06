﻿using ChobiAssets.KTP;
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
    // AI
    private AI ai;
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

    // GUI
    // 击杀提示
    public Texture2D killUI;
    private float killUIStartTime = float.MinValue;

    // 准心
    public Texture2D centerSight;
    public Texture2D tankSight;
    // 生命条
    public Texture2D hpBarBg;
    public Texture2D hpBar;

    void Start()
    {
        rigi = gameObject.GetComponent<Rigidbody>();
        rigi.centerOfMass = new Vector3(rigi.centerOfMass.x, -0.1f, rigi.centerOfMass.z);
        turret = transform.Find("turret");
        gun = turret.Find("gun");


        wheels = transform.Find("wheels");
        tracks = transform.Find("tracks");

        // 人工智能
        if (ctrlType == CtrlType.computer)
        {
            Debug.Log(" set type computer");
            ai = gameObject.AddComponent<AI>();
            ai.tank = this;
        }

    }

    // Update is called once per frame
    void Update()
    {
        // 修改重心

        //  玩家控制操控
        // 获取用户输入信息，给motor，streer，brakeTorque赋值
        PlayerCtrl();
        ComputerCtrl();
        NoneCtrl();
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

        // 根据turretRotTarget,turretRollTarget
        // 来设置GameObject turret炮塔, gun炮管旋转
        TurretRotation();
        TurretRoll();

    }
    private void OnGUI()
    {
        if (ctrlType != CtrlType.player)
            return;
        DrawSight();
        DrawHp();
        DrawKillUI();
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
        // 设置好motor, streering, brakeTorque
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

        // 设置炮台角度，目的是设置好turretRotTarget， turretRollTarget
        TargetSignPos();
        // 发射炮弹
        if (Input.GetMouseButton(0))
            Shoot();
    }

    public void ComputerCtrl()
    {
        if (ctrlType != CtrlType.computer)
            return;

        // 设置好motor, streering, brakeTorque

        steering = ai.GetSteering();
        motor = ai.GetMotor();
        brakeTorque = ai.GetBrakeTorque();

        // 旋转炮管对准目标，如果没有目标回到原来的角度
        Vector3 rot = ai.GetTurretTarget();
        turretRotTarget = rot.y;
        turretRollTarget = rot.x;

        // 射击
        if (ai.isShoot())
            Shoot();
    }
    public void NoneCtrl()
    {
        if (ctrlType != CtrlType.none)
            return;
        motor = 0;
        steering = 0;
        brakeTorque = MaxBrakeTorque / 2;
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
        GameObject bulletObj = (GameObject)Instantiate(bullet, pos, gun.rotation);
        Bullet bulletCmp = bulletObj.GetComponent<Bullet>();
        if (bulletCmp != null)
            bulletCmp.attackTank = this.gameObject;

        lastShootTime = Time.time;
        // 自己打自己
        //BeAttacked(30f);

    }
    public void BeAttacked(float att, GameObject attackTank)
    {
        if (hp <= 0)
            return;
        if (hp > 0)
        {
            hp -= att;
            if (ai != null)
            {
                ai.OnAttacked(attackTank);
            }
        }
        if (hp <= 0)
        {
            GameObject destoryObj = (GameObject)Instantiate(destoryEffect);
            destoryObj.transform.SetParent(transform, false);
            destoryObj.transform.localPosition = new Vector3(0, 0, 0.3f);
            ctrlType = CtrlType.none;

            // 显示击杀提示
            if (attackTank != null)
            {
                Tank tankCmp = attackTank.GetComponent<Tank>();
                if (tankCmp != null && tankCmp.ctrlType == CtrlType.player)
                    tankCmp.StartDrawKill();
            }
        }

    }
    public void StartDrawKill()
    {
        killUIStartTime = Time.time;
    }
    public void TargetSignPos()
    {
        Vector3 hitPoint = Vector3.zero;
        RaycastHit raycastHit;

        Vector3 centerVec = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = Camera.main.ScreenPointToRay(centerVec);

        // 射线检测, 获取HitPoint
        if (Physics.Raycast(ray, out raycastHit, 400.0f))
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

    }

    public Vector3 CalExplodePoint()
    {
        // 碰撞信息和碰撞点
        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;

        // 沿着炮管方向的射线
        Vector3 pos = gun.position + gun.forward * 5;
        Ray ray = new Ray(pos, gun.forward);

        // 射线检测
        if (Physics.Raycast(ray, out hit, 400.0f))
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(400);
        }
        //Transform explodeCube = GameObject.Find("ExplodeCube").transform;
        //explodeCube.position = hitPoint;

        return hitPoint;
    }

    public void DrawSight()
    {
        // 实际射击位置
        Vector3 explodePoint = CalExplodePoint();
        // 获取坦克准心
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(explodePoint);

        // 绘制坦克准心
        Rect tankRect = new Rect(screenPoint.x - tankSight.width / 2,
            Screen.height - screenPoint.y - tankSight.height / 2,
            tankSight.width, tankSight.height);
        GUI.DrawTexture(tankRect, tankSight);

        // 绘制中心准心
        Rect centerRect = new Rect(Screen.width / 2 - centerSight.width / 2,
            Screen.height / 2 - centerSight.height / 2,
            centerSight.width,
            centerSight.height
            );
        GUI.DrawTexture(centerRect, centerSight);

    }
    public void DrawHp()
    {
        // 底框
        Rect bgRect = new Rect(30, Screen.height - hpBarBg.height - 15,
            hpBarBg.width, hpBarBg.height);
        GUI.DrawTexture(bgRect, hpBarBg);

        // 指示条
        float width = hp * 102 / maxHp;
        Rect heRect = new Rect(bgRect.x + 29, bgRect.y + 9, width, hpBar.height);
        GUI.DrawTexture(heRect, hpBar);
        // 文字
        string text = Mathf.Ceil(hp).ToString() + "/" + Mathf.Ceil(maxHp).ToString();
        Rect textRect = new Rect(bgRect.x + 80, bgRect.y - 10, 50, 50);
        GUI.Label(textRect, text);
    }
    private void DrawKillUI()
    {
        if (Time.time - killUIStartTime < 1f)
        {
            Rect rect = new Rect(Screen.width / 2 - killUI.width / 2, 30,
                killUI.width, killUI.height);
            GUI.DrawTexture(rect, killUI);
        }
    }
}
