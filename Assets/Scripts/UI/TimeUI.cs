using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    //日夜交替
    public RectTransform dayNightImage;
    //时间格子的父物体
    public RectTransform clockParent;
    //季节图片
    public Image seasonImage;
    //日期
    public TextMeshProUGUI dateText;
    //时间
    public TextMeshProUGUI timeText;
    //季节的所有图片
    public Sprite[] seasonSprites;
    
    public List<GameObject> clockBlocks =new List<GameObject>();

    private void Awake()
    {
        //获取所有时间格子对象
        for (int i = 0; i < clockParent.childCount; i++)
        {
            clockBlocks.Add(clockParent.GetChild(i).gameObject);
            clockBlocks[i].gameObject.SetActive(false);
        }
        
    }

    private void OnEnable()
    {
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.GameDateEvent += OnGameDateEvnet;
    }
    /// <summary>
    /// 当分钟发生变化后更新UI中小时和分钟的事件
    /// </summary>
    /// <param name="minute">当前分钟</param>
    /// <param name="hour">当前小时</param>
    private void OnGameMinuteEvent(int minute, int hour,int day,Season season)
    {
        timeText.text = hour.ToString("00") + ":" + minute.ToString("00");
    }
    /// <summary>
    /// 当小时发生变化后更新UI中年月日和季节的事件
    /// </summary>
    /// <param name="hour">当前小时/param>
    /// <param name="day">当天是第几天</param>
    /// <param name="month">当前月份</param>
    /// <param name="year">当前年份</param>
    /// <param name="season">当前季节</param>
    private void OnGameDateEvnet(int hour, int day, int month, int year, Season season)
    {
        //更新年月日
        dateText.text = "第"+year + "年" + month.ToString("00") + "月" + day.ToString("00") + "日";
        //更新季节图片
        seasonImage.sprite = seasonSprites[(int) season];
        SwitchHourImage(hour);
        SwitchDayNightImage(hour);
    }

    /// <summary>
    /// 更新时间格子的方法
    /// </summary>
    /// <param name="hour"></param>
    private void SwitchHourImage(int hour)
    {
        int index = hour / 4+1;
        if (index == 0)
        {
            foreach (var item in clockBlocks)
            {
                item.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < clockBlocks.Count; i ++ )
            {
                if (i < index)
                {
                    clockBlocks[i].SetActive(true);
                }
                else
                {
                    clockBlocks[i].SetActive(false);
                }
            }
        }
    }

    public void SwitchDayNightImage(int hour)
    {
        var target = new Vector3(0,0,hour*15-90);
        dayNightImage.DORotate(target, 1f,RotateMode.Fast);
    }
    private void OnDisable()
    {
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.GameDateEvent -= OnGameDateEvnet;
    }
}
