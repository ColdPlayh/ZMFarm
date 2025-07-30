using System;
using UnityEngine;


public class LightManager : MonoBehaviour
{
    //存放场景中所有的灯光
    private LightControl[] sceneLights;
    //当前的灯光
    private LightShift currentLightShift;
    //当前的季节
    private Season currentSeason;

    //切换灯光的时间间隔
    private float timeDifference=Settings.lightChangeDuration;

    private void OnEnable()
    {
        EventHandler.AfterPlayerMoveEvent += OnAfterSceneLoadEvent;
        EventHandler.LightShiftChangeEvent += OnLightShiftChangeEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;

    }

    private void OnDisable()
    {
        EventHandler.AfterPlayerMoveEvent -= OnAfterSceneLoadEvent;
        EventHandler.LightShiftChangeEvent -= OnLightShiftChangeEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        //游戏一开始为早上的灯光
        currentLightShift = LightShift.Morning;
    }

    //切换灯光的事件
    private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        currentSeason = season;
        this.timeDifference = timeDifference;
        //如果当前的灯光不等于目标的灯光
        if (currentLightShift != lightShift)
        {
            
            currentLightShift = lightShift;
            //切换灯光效果
            foreach (LightControl light in sceneLights)
            {
                light.ChangeLightShift(currentSeason,currentLightShift,timeDifference);
            }
        }
    }

    //OPTIMIZE:改一个事件
    private void OnAfterSceneLoadEvent()
    {
        sceneLights = FindObjectsOfType<LightControl>();
        foreach (var light in sceneLights)
        {
            //TODO：切换灯光
            light.ChangeLightShift(currentSeason,currentLightShift,timeDifference);
        }
    }
}