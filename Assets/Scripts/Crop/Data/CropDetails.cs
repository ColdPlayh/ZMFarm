using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class CropDetails
{
    /// <summary>
    /// 种子ID
    /// </summary>
    public int seedItemID;
    [Header("种子不同阶段需要的时间")] 
    public int[] growthDays;

    /// <summary>
    /// 总共生长的时间
    /// </summary>
    public int TotalGrowthDays
    {
        get
        {
            int amount = 0;
            foreach (var days in growthDays)
            {
                amount += days;
            }

            return amount;
        }
    }

    [FormerlySerializedAs("gorwthPrefabs")] [Header("不同生长阶段的Prefab")] 
    public GameObject[] growthPrefabs;
    [Header("不同阶段的图片")]
  
    public Sprite[] growthSprites;

    [Header("可种植的季节")] public Season[] canPlantedSeasons;

    [Space] [Header("收割工具")] 
    public int[] harvestToolItemID;

    [Header("每种工具的使用次数")] 
    public int[] requireActionCount;

    [Header("收割后该植物转换的物品ID")] 
    public int transferItemID;

    [Space] [Header("收割果实的信息")]
    public int[] producedItemID;

    public int[] producedMaxAmount;
    public int[] produceMinAmount;
    public Vector2 produceRadius;
    [Header("再次生长的时间")] 
    
    
    public int daysInRegrow;
    /// <summary>
    /// 可以重新生长的次数
    /// </summary>
    public int regrowTimes;
    [Header("Option")]
    public bool generateAtPlayerPosition;

    public bool hasAnimation;
    public bool hasParticalEffect;
    
    [Header("粒子效果")]
    //TODO:不同的特效
    public ParticleEffectType particleEffectType;

    public Vector3 effectPos;

    public SoundName soundEffect;

    #region 功能方法

    public bool CheckToolAvailable(int toolID)
    {
        foreach (var tool in harvestToolItemID)
        {
            if (tool == toolID) return true;
            
        }
        return false;
    }

    /// <summary>
    /// 获取收获当前作物需要使用工具的次数
    /// </summary>
    /// <param name="toolID"></param>
    /// <returns></returns>
    public int GetTotalRequireCount(int toolID)
    {
        for (int i = 0; i < harvestToolItemID.Length; i++)
        {
            if (harvestToolItemID[i] == toolID)
            {
                return requireActionCount[i];
            }
            
        }

        return -1;
    }
    #endregion

}
