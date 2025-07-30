using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存储常量以及静态的变量等
/// </summary>
public class Settings
{
    
    /// <summary>
    /// 树木完成透明度变化所需的时间
    /// </summary>
    public const float ItemFadeDuration=0.35f;
    /// <summary>
    /// 树木半透明的目标透明度
    /// </summary>
    public const float TargetAlpha = 0.45f;
    /// <summary>
    /// 起始Item ID
    /// </summary>
    public const int OrginItemID = 1001;

    /// <summary>
    /// 游戏中每一秒需要的现实时间 数值越小时间越快
    /// </summary>
    public const float secondThreshold = 0.02f;

    /// <summary>
    /// 多久秒一分钟
    /// </summary>
    public const int secondHold = 59;
    /// <summary>
    /// 多少分钟一小时
    /// </summary>
    public const int minuteHold = 59;
    /// <summary>
    /// 多少小时一天
    /// </summary>
    public const int hourHold = 23;
    /// <summary>
    /// 多少天一个月
    /// </summary>
    public const int dayHold = 10;

    public const int monthHold = 12;
    /// <summary>
    /// 多少月一个季节
    /// </summary>
    public const int seasonHold = 3;

    public const int reapAmount = 3;

    //Animator中的parameterName
    public const string AnimFloatParameterInputX = "InputX";
    public const string AnimFloatParameterInputY = "InputY";
    public const string AnimBoolParameterIsMoving = "IsMoving";
    public const string AnimFloatParameterMouseX = "MouseX";
    public const string AnimFloatParameterMouseY = "MouseY";
    public const string AnimTriggerParaUseTool = "UseTool";
    
    //所有场景的名称
    public const string FiledSceneName = "01.Field";
    public const string HomeSceneName = "02.Home";
    public const string StallSceneName = "03.Stall";
    public const string SeaSceneName = "04.Sea";
    
    //UI相关的
    public const float fadeDuration = 0.75f;
    
    //NPC相关
    //网格边长
    public const float gridCellSize = 1f;
    //网格对角线长度
    public const float gridCellDiagonalSize = 1.4f;
    //一个像素的大小
    public const float pixelSize = 0.05f;
    //npc执行动作的时间间隔
    public const float animationBreakTime = 5f;
    public const int maxGridSize = 9999;

    //灯光相关
    //多少秒切换一次灯光
    public const float lightChangeDuration = 25;
    //早上的时间
    public static TimeSpan morningTime = new TimeSpan(5, 0, 0);
    //晚上的时间
    public static TimeSpan nightTime = new TimeSpan(19, 0, 0);


    #region 保存相关数据

    //玩家初始的坐标
    public static Vector3 playerStartPos = new Vector3(2, -3, 0);
    public static int playerStartMoney = 1000;

    #endregion

}
