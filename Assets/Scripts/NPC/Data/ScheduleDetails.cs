using System;
using UnityEngine;


[Serializable]
public class ScheduleDetails : IComparable<ScheduleDetails>
{
    public int hour, minute, day;

    /// <summary>
    /// 优先级 优先级越小越先执行
    /// </summary>
    public int priority;

    //季节1
    public Season season;

    //目标场景
    public string targetScene;

    //目标网格位置
    public Vector2Int targetGridPosition;

    //
    public AnimationClip clipAtStop;
    //可以被交互
    public bool isInteractable;

    public int ExecuteTime => (hour * 100) + minute;

    public ScheduleDetails(int hour, int minute, int day, int priority, Season season, string targetScene, Vector2Int targetGridPosition, AnimationClip clipAtStop, bool interactable)
    {
        this.hour = hour;
        this.minute = minute;
        this.day = day;
        this.priority = priority;
        this.season = season;
        this.targetScene = targetScene;
        this.targetGridPosition = targetGridPosition;
        this.clipAtStop = clipAtStop;
        this.isInteractable = interactable;
    }

    public int CompareTo(ScheduleDetails other)
    {
        if (ExecuteTime == other.ExecuteTime)
        {
            if (priority > other.priority) return 1;
            else return -1;
        }
        else if (ExecuteTime > other.ExecuteTime)
        {
            return 1;
        }
        else if (ExecuteTime < other.ExecuteTime)
        {
            return -1;
        }

        return 0;
    }
}
