using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using LYFarm.Save;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>,ISaveable
{
    /// <summary>
    /// 游戏时间中的年月日、时分秒
    /// </summary>
    private int gameSecond, gameMinute, gameHour, gameDay, gameMonth, gameYear;

    /// <summary>
    /// 游戏所处的季节
    /// </summary>
    private Season gameSeason = Season.春天;

    /// <summary>
    /// 当前季节剩余几个月
    /// </summary>
    private int monthInSeason = 3;

    //时间差
    public float timeDifference;

    /// <summary>
    /// 是否暂停时间流逝
    /// </summary>
    public bool gameClockPause;

    private float tikTime;

    public TimeSpan timeSpan => new TimeSpan(gameHour, gameMinute, gameSecond);
    
    
    protected override void Awake()
    {
        base.Awake();
        
    }
    
    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void Start()
    {
        //注册
        ISaveable saveable = this;
        saveable.RegisterSaveable();
        gameClockPause = true;
        // EventHandler.CallGameMinuteEvent(gameSecond,gameHour,gameDay,gameSeason);
        // EventHandler.CallGameDateEvent(gameHour,gameDay,gameMonth,gameYear,gameSeason);
        // //切换灯光
        // EventHandler.CallLightShiftChangeEvent(gameSeason,GetCurrentLightShift(),timeDifference);
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        gameClockPause = true;
    }

    private void OnStartNewGameEvent(int obj)
    {
        //初始化时间
        NewGameTime();
        gameClockPause = false;
    }

    //游戏暂停
    private void OnUpdateGameStateEvent(GameState gameState)
    {
        //暂停时间
        gameClockPause = gameState == GameState.Pause;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        gameClockPause = true;
    }
    private void OnAfterSceneLoadedEvent()
    {
        gameClockPause = false;
        EventHandler.CallGameMinuteEvent(gameSecond,gameHour,gameDay,gameSeason);
        EventHandler.CallGameDateEvent(gameHour,gameDay,gameMonth,gameYear,gameSeason);
        //切换灯光
        EventHandler.CallLightShiftChangeEvent(gameSeason,GetCurrentLightShift(),timeDifference);
    }

   

    private void Update()
    {
        if (!gameClockPause)
        {
            tikTime += Time.deltaTime;
            if (tikTime >= Settings.secondThreshold)
            {
                tikTime -=Settings.secondThreshold;
                UpdateGameTime();
            }
        }

        //测试语句
        if (Input.GetKey(KeyCode.T))
        {
            for (int i = 0; i < 60; i++)
            {
                UpdateGameTime();
            }
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            for (int i = 0; i < 60*30; i++)
            {
                UpdateGameTime();
            }
        }
        
        //测试语句
        if (Input.GetKeyDown(KeyCode.G))
        {
            gameDay++;
            EventHandler.CallGameDayEvent(gameDay,gameSeason);
            EventHandler.CallGameDateEvent(gameHour,gameDay,gameMonth,gameYear,gameSeason);
        }
    }

    //初始化时间
    private void NewGameTime()
    {
        gameSecond = 0;
        gameMinute = 0;
        gameHour = 7;
        gameDay = 1;
        gameMonth = 1;
        gameYear = 1;
        gameSeason = Season.春天;

    }
    private void UpdateGameTime()
    {
        //增加一秒
        gameSecond++;
        //如果达到秒数上限
        if (gameSecond > Settings.secondHold)
        {
            //加一分钟
            gameMinute++;
            //重置秒
            gameSecond = 0;
            if (gameMinute > Settings.minuteHold)
            {
                gameHour++;
                gameMinute = 0;
                if (gameHour > Settings.hourHold)
                {
                    gameDay++;
                    gameHour = 0;
                    if (gameDay > Settings.dayHold)
                    {
                        gameDay = 1;
                        gameMonth++;
                        if (gameMonth > Settings.monthHold)
                        {
                            gameMonth = 1;
                            //当前季节剩余月份减少
                            monthInSeason--;
                            //TODO：季节的逻辑
                            //剩余月份为0
                            if (monthInSeason == 0)
                            {
                                //重置当前季节剩余月份
                                monthInSeason = 3;
                                //获取季节的序号
                                int seasonNumber = (int) gameSeason;
                                //季节的序号自增（进入下一个季节）
                                seasonNumber++;
                                //如果大于预设的季节数量
                                if (seasonNumber > Settings.seasonHold)
                                {
                                    //重置季节
                                    seasonNumber = 0;
                                    //增加一年
                                    gameYear++;
                                }

                                //设置当前的季节
                                gameSeason = (Season) seasonNumber;
                            }

                            //年数限制
                            if (gameYear > 9999)
                            {
                                gameYear = 0;
                            }

                        }
                    }
                    //日期发生变化的时候调用的事件
                    EventHandler.CallGameDayEvent(gameDay,gameSeason);
                    
                }
                //小时发生变得时候调用关于日 月 年 季节的相关事件
                EventHandler.CallGameDateEvent(gameHour,gameDay,gameMonth,gameYear,gameSeason);
            }
            //分钟发生变化的时候调用 关于分和小时的相关事件
            EventHandler.CallGameMinuteEvent(gameMinute,gameHour,gameDay,gameSeason);
            //切换灯光
            EventHandler.CallLightShiftChangeEvent(gameSeason,GetCurrentLightShift(),timeDifference);

        }
        
        
        
     
    }

    private LightShift GetCurrentLightShift()
    {
        //白天
        if (timeSpan >= Settings.morningTime && timeSpan <= Settings.nightTime)
        {
            //我们现在的事件距离早上过了多少分钟
            timeDifference = (float) (timeSpan - Settings.morningTime).TotalMinutes;
            return LightShift.Morning;
        }

        //TODO：添加中午 黄昏 晚上
        //晚上
        if (timeSpan <= Settings.morningTime || timeSpan >= Settings.nightTime)
        {
            timeDifference = Math.Abs((float) (timeSpan - Settings.nightTime).TotalMinutes);
            return LightShift.Night;
        }

        return LightShift.Morning;

    }

    public string Guid => GetComponent<DataGUID>().guid;
    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.timeDict = new Dictionary<string, int>();
        //保存时间
        saveData.timeDict.Add("GameYear",gameYear);
        saveData.timeDict.Add("GameSeason",(int)gameSeason);
        saveData.timeDict.Add("GameMonth",gameMonth);
        saveData.timeDict.Add("GameDay",gameDay);
        saveData.timeDict.Add("GameHour",gameHour);
        saveData.timeDict.Add("GameMinute",gameMinute);
        saveData.timeDict.Add("GameSecond",gameSecond);
        return saveData;
    }

    public void RestoreData(GameSaveData gameSaveData)
    {
        //读取时间
        gameYear = gameSaveData.timeDict["GameYear"];
        gameSeason = (Season)gameSaveData.timeDict["GameSeason"];
        gameMonth = gameSaveData.timeDict["GameMonth"];
        gameDay = gameSaveData.timeDict["GameDay"];
        gameHour = gameSaveData.timeDict["GameHour"];
        gameMinute = gameSaveData.timeDict["GameMinute"];
        gameSecond = gameSaveData.timeDict["GameSecond"];
    }
}
