using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityScript.Scripting.Pipeline;

public class Path
{
    // 所有路点
    public Vector3[] waypoints;
    // 当前路点索引
    public int index = -1;
    // 当前的路点
    public Vector3 waypoint;
    // 是否循环
    bool isLoop = false;
    // 达到误差
    public float deviation = 6;
    // 是否完成
    public bool isFinish = false;

    public bool IsReach(Transform trans)
    {
        Vector3 pos = trans.position;
        float distance = Vector3.Distance(waypoint, pos);
        return distance < deviation;
    }

    public void NextWaypoint()
    {
        if (index < 0)
            return;
        if(index < waypoints.Length - 1)
        {
            Debug.Log("更新下一个路点");
            index++;
        }
        else
        {
            if (isLoop)
                index = 0;
            else
                isFinish = true;
        }
        waypoint = waypoints[index];
    }

    public void InitByObj(GameObject obj, bool isLoop = false)
    {
        int length = obj.transform.childCount;

        if(length == 0)
        {
            waypoints = null;
            index = -1;
            Debug.LogWarning("Path.InitByObj: length == 0");
            return;
        }
        Debug.Log("Path.InitByObj: length " + length.ToString());

        waypoints = new Vector3[length];
        for(int i = 0;i < length;i++)
        {
            Transform trans = obj.transform.GetChild(i);
            waypoints[i] = trans.position;
        }
        index = 0;
        waypoint = waypoints[index];
        this.isLoop = isLoop;
        isFinish = false;
    }
}
