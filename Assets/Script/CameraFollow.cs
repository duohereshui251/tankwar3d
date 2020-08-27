using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 距离坦克的距离
    public float distance = 10;
    public float maxDistance = 20f;
    public float minDistance = 5f;
    public float zoomSpeed = 0.2f;
    // 横向角度
    public float rot = 0;
    public float rotSpeed = 0.1f;

    // 纵向角度
    // 默认纵向角度为30度
    public float roll = 20f * Mathf.PI * 2 / 360;
    private float rollSpeed = 0.1f;
    private float maxRoll = 50f * Mathf.PI * 2 / 360;
    private float minRoll = -10f * Mathf.PI * 2 / 360;
    private GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        //target = GameObject.Find("tank");
        SetTarget(GameObject.Find("tank"));
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null)
            return;
        if (Camera.main == null)
            return;
        // 镜头距离
        Zoom();
        // 旋转
        Rotate();
        Roll();
        Vector3 targetPos = target.transform.position;
        Vector3 cameraPos;
        float d = distance * Mathf.Cos(roll);
        float height = distance * Mathf.Sin(roll);
        cameraPos.x = targetPos.x + d * Mathf.Cos(rot);
        cameraPos.z = targetPos.z + d * Mathf.Sin(rot);
        cameraPos.y = targetPos.y + height;

        Camera.main.transform.position = cameraPos;

        Camera.main.transform.LookAt(target.transform);
    }

    public void SetTarget(GameObject target)
    {
        if (target.transform.Find("cameraPoint") != null)
        {
            this.target = target.transform.Find("cameraPoint").gameObject;
        }
        else
        {
            this.target = target;
        }
    }
    void Rotate()
    {
        float w = Input.GetAxis("Mouse X") * rotSpeed;

        rot -= w;
    }
    void Roll()
    {
        float w = Input.GetAxis("Mouse Y") * rollSpeed;
        roll -= w;
        if (roll > maxRoll)
            roll = maxRoll;
        if (roll < minRoll)
            roll = minRoll;
    }

    void Zoom()
    {
        // 鼠标滚轮控制摄像头距离
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (distance > minDistance)
                distance -= zoomSpeed;

        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (distance < maxDistance)
                distance += zoomSpeed;
        }
    }

}
