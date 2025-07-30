using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LYFarm.Save
{
    /// <summary>
    /// 游戏中一个进度中的数据（一个进度会有很多个）
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>
        /// 储存场景名称
        /// </summary>
        public string dataSceneName;
        /// <summary>
        /// 储存玩家和npc位置的字典
        /// </summary>
        public Dictionary<string, SerializableVector3> characterPosDict;
        /// <summary>
        /// 储存场景中所有的sceneItem
        /// </summary>
        public Dictionary<string, List<SceneItem>> sceneItemDict;

        /// <summary>
        /// 储存场景中所有建造的家具
        /// </summary>
        public Dictionary<string, List<SceneFurniture>> sceneFurnitureDict;

        /// <summary>
        /// 保存每个瓦片数据的字典
        /// <para>key:场景名称:坐标</para>
        /// <para>value:该瓦片的数据</para>>
        /// </summary>
        public  Dictionary<string, TileDetails> tileDetailsDict;

        /// <summary>
        /// 存放所有场景是否第一次加载
        /// </summary>
        public Dictionary<string, bool> SceneIsFristLoadDict;

        /// <summary>
        /// 储存所有的库存容器（背包箱子等）
        /// </summary>
        public Dictionary<string, List<InventoryItem>> inventoryDict;

        /// <summary>
        /// 存储时间
        /// </summary>
        public Dictionary<string, int> timeDict;


        /// <summary>
        /// 玩家的金币
        /// </summary>
        /// <returns></returns>
        public int playerMoney;
        //npc相关
        /// <summary>
        /// npc目标场景
        /// </summary>
        public string targetScene;
        /// <summary>
        /// npc是否可互动
        /// </summary>
        public bool interactable;
        /// <summary>
        /// npc的animation动作
        /// </summary>
        public int animationInstanceID;
        





    }
}

