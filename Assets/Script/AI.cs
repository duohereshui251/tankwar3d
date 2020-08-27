using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public Tank tank;
    public enum Status
    {
        Patrol,
        Attack,
    }
    private Status status = Status.Patrol;
    // Start is called before the first frame update
    void Start()
    {

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

    }
    public void ChangeStatus(Status status)
    {
        if(status == Status.Patrol)
        {
            PatrolStart();
        }else if (status == Status.Attack)
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
}
