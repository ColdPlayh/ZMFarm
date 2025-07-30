using System;
using System.Collections.Generic;
using LYFarm.Save;
using LYFarm.Transition;
using UnityEngine;

namespace LYFarm.Save
{
    /// <summary>
    /// 游戏中一个进度的数据类型
    /// </summary>
    public class DataSlot
    {
        /// <summary>
        /// key：guid（比如itemManger保存的数据的key就是它的guid）
        /// value：保存的savedata（比如itemManger保存的savedata）
        /// </summary>
        public Dictionary<string, GameSaveData> saveDataDict = new Dictionary<string, GameSaveData>();

        //获取时间的字符串
        public string DataTime
        {
            get
            {
                var key = TimeManager.Instance.Guid;
                if (saveDataDict.ContainsKey(key))
                {
                    GameSaveData timeSaveData = saveDataDict[key];
                    return timeSaveData.timeDict["GameYear"] + "年/" + (Season) timeSaveData.timeDict["GameSeason"] + "/" +
                           timeSaveData.timeDict["GameMonth"] + "月/" + timeSaveData.timeDict["GameDay"] + "日/";
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        //获取场景名称的字符串
        public string GameScene
        {
            get
            {
                var key = TransitionManager.Instance.Guid;
                if (saveDataDict.ContainsKey(key))
                {
                    GameSaveData transitionSaveData = saveDataDict[key];
                    return transitionSaveData.dataSceneName switch
                    {
                        Settings.FiledSceneName=>"ColdPlay的农场",
                        Settings.HomeSceneName=>"ColdPlay的家",
                        Settings.StallSceneName=>"温泉小径",
                        Settings.SeaSceneName=>"海边",
                        _=>String.Empty
                    };
                }
                else
                {
                    return String.Empty;
                }
                
            }
        }
        
    }

}
