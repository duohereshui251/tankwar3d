using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class AI : MonoBehaviour
{
    public Tank tank;
    // 路径
    private Path path = new Path();
    public enum Status
    {
        Patrol,
        Attack,
    }
    private Status status = Status.Patrol;

    // 锁定的坦克
    private GameObject target;
    // 视野范围
    private float sightDistance = 30;
    // 上一次搜寻时间
    private float lastSearchTargetTime = 0;
    // 搜寻间隔
    private float searchTargetInterval = 3;


    void Start()
    {
        InitWaypoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (tank.ctrlType != Tank.CtrlType.computer)
            return;

        if (status == Status.Patrol)
            PatrolUpdate();
        else if (status == Status.Attack)
            AttackUpdate();

        // 更新目标
        TargetUpdate();
        // 行走
        if (path.IsReach(transform))
        {
            Debug.Log("达到一个路点");
            path.NextWaypoint();
        }

    }
    public void ChangeStatus(Status status)
    {
        if (status == Status.Patrol)
        {
            PatrolStart();
        }
        else if (status == Status.Attack)
        {
            AttackStart();
        }

    }

    // 巡逻开始
    void PatrolStart()
    {

    }
    // 攻击开始
    void AttackStart()
    {

    }
    // 巡逻中
    void PatrolUpdate()
    {

    }
    void AttackUpdate()
    {

    }
    // 搜寻目标
    void TargetUpdate()
    {
        float interval = Time.time - lastSearchTargetTime;
        if (interval < searchTargetInterval)
            return;
        lastSearchTargetTime = Time.time;

        if (target != null)
            HasTarget();
        else
            NoTarget();
    }

    void HasTarget()
    {
        Tank targetTank = target.GetComponent<Tank>();
        Vector3 pos = transform.position;
        Vector3 targetPos = targetTank.transform.position;
        if (targetTank.ctrlType == Tank.CtrlType.none)
        {
            Debug.Log("目标死亡， 丢失目标");
            target = null;
        }
        else if (Vector3.Distance(pos, targetPos) > sightDistance)
        {
            Debug.Log("距离过远， 丢失目标");
            target = null;
        }

    }
    void NoTarget()
    {
        float minHp = float.MaxValue;

        GameObject[] targets = GameObject.FindGameObjectsWithTag("tank");
        for (int i = 0; i < targets.Length; i++)
        {
            Tank tank = targets[i].GetComponent<Tank>();

            if (tank == null)
                continue;
            if (targets[i] == gameObject)
                continue;
            if (tank.ctrlType == Tank.CtrlType.none)
                continue;

            Vector3 pos = transform.position;
            Vector3 targetPos = targets[i].transform.position;

            if (Vector3.Distance(pos, targetPos) > sightDistance)
                continue;

            if (minHp > tank.hp)
            {
                target = tank.gameObject;
                break;
            }
        }
        if (target != null)
            Debug.Log("获取目标 " + target.name);

    }
    public void OnAttacked(GameObject attackTank)
    {
        target = attackTank;
    }

    // 对外接口
    public Vector3 GetTurretTarget()
    {
        if (target != null)
        {
            Vector3 pos = transform.position;
            Vector3 targetPos = target.transform.position;
            Vector3 vec = targetPos - pos;
            return Quaternion.LookRotation(vec).eulerAngles;
        }
        else
        {
            float y = transform.eulerAngles.y;
            Vector3 rot = new Vector3(0, y, 0);
            return rot;
        }


    }

    public bool isShoot()
    {
        if (target == null)
            return false;
        float turretRoll = tank.turret.eulerAngles.y;
        float angle = turretRoll - GetTurretTarget().y;
        if (angle < 0) angle += 360;

        if (angle < 30 || angle > 330)
            return true;
        else
            return false;
    }
    // 路径相关
    void InitWaypoint()
    {
        GameObject obj = GameObject.Find("WaypointContainer");
        if (obj)
            path.InitByObj(obj,true);
    }
    public float GetSteering()
    {
        if (tank == null)
            return 0;

        Vector3 itp = transform.InverseTransformPoint(path.waypoint);

        // 右转
        if (itp.x > path.deviation / 5)
            return tank.maxSteeringAngle;
        else if (itp.x < -path.deviation / 5) // 左转
            return -tank.maxSteeringAngle;
        else
            return 0;
    }

    public float GetMotor()
    {
        if (tank == null)
            return 0;
        Vector3 itp = transform.InverseTransformPoint(path.waypoint);
        float x = itp.x;
        float z = itp.z;
        float r = 6;
        if (z < 0 && Mathf.Abs(x) < -z && Mathf.Abs(x) < r)
            return -tank.MaxMotorTorque;
        else
            return tank.MaxMotorTorque;
    }
    public float GetBrakeTorque()
    {
        if (path.isFinish)
            return tank.MaxMotorTorque;
        else
            return 0;
    }

}
