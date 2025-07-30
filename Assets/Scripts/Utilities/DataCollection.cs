using System;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class ItemDetails
{
    /// <summary>
    /// 物品id
    /// </summary>
    /// <returns></returns>
    public int itemID;
    /// <summary>
    /// 物品名称
    /// </summary>
    public string itemName;
    /// <summary>
    /// 物品类型
    /// </summary>
    public ItemType itemType;
    /// <summary>
    /// 编辑器中|建造图纸的图标
    /// </summary>
    public Sprite itemIcon;
    /// <summary>
    /// 在Scene中显示的图片
    /// </summary>
    public Sprite itemOnWorldSprite;
    /// <summary>
    /// 物品描述
    /// </summary>
    public string itemDescription;
    /// <summary>
    /// 使用范围
    /// </summary>
    public int itemUseRadius;

    /// <summary>
    /// 是否可以捡起
    /// </summary>
    public bool canPickedUp;
    /// <summary>
    /// 是否可以被丢掉
    /// </summary>
    public bool canDropped;
    /// <summary>
    /// 是否可以被搬起
    /// </summary>
    public bool canCarried;
    /// <summary>
    /// 物品价格
    /// </summary>
    public int itemPrice;
    /// <summary>
    /// 售卖折扣比 0-1
    /// </summary>
    [Range(0, 1)] 
    public float sellPercentage;

    public bool canSell
    {
        get
        {
            if (itemType == ItemType.Seed || itemType == ItemType.Commodity || itemType == ItemType.Furniture)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
[Serializable]
//slot的属性
public struct InventoryItem
{
    public int itemID;
    public int itemAmount;
    
}
/// <summary>
/// 主角的动画类型
/// </summary>
[Serializable]
public class AnimatorType
{
    public PartType partType;
    public PartName partName;
    public AnimatorOverrideController overrideController;

}
/// <summary>
/// 可以序列化的Vector3
/// </summary>
[Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3()
    {
        
    }
    public SerializableVector3(Vector3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int) x, (int) y);
    }

    public Vector3Int ToVector3Int()
    {
        return new Vector3Int((int) x, (int) y, (int) z);
    }
    
} 
/// <summary>
/// 场景中的item
/// <para>itemID：item的id</para>
/// <para>position：item在场景中的位置</para>
/// </summary>
[Serializable]
public class SceneItem
{
    public int itemID;
    public SerializableVector3 position;
}

[Serializable]
public class SceneFurniture
{
    //TODO:box序号
    public int furnitureID;
    public SerializableVector3 position;

    public int boxIndex;
}
/// <summary>
/// 每个瓦片的属性
/// <para>tileCoordinate:瓦片的坐标</para>
/// <para>girdType:瓦片类型（dig drop place obstacle）</para>
/// <para>boolTypeValue:对应的bool值</para>
/// </summary>
[Serializable]
public class TileProperty
{
    //瓦片的位置
    public Vector2Int tileCoordinate;
    //当前瓦片类型
    public GirdType girdType;
    //
    public bool boolTypeValue;
}

/// <summary>
/// 瓦片数据
/// </summary>
public class TileDetails
{
    //属性相关
    public int girdX, girdY;
    public bool canDig;
    public bool canDropItem;
    public bool canPlaceFurniture;
    public bool isNpcObstacle;
    //种植相关
    /// <summary>
    /// 距离挖掘多少天  默认为-1:代表未被挖掘
    /// </summary>
    public int daysSinceDig=-1;
    /// <summary>
    /// 距离浇水多少天  默认为-1:代表没有浇水
    /// </summary>
    public int daysSinceWatered=-1;
    /// <summary>
    /// 种植的种子的数据
    /// </summary>
    public int seedItemID=-1;
    /// <summary>
    /// 距离上次收获多少天
    /// </summary>
    public int daySinceLastHarvest=-1;
    /// <summary>
    /// 生长日期
    /// </summary>
    public int growthDays = -1;
}
[Serializable]
public class NPCPosition
{
    public Transform npc;
    public string startScene;
    public Vector3 position;
}

[Serializable]
public class SceneRoute
{
    public string fromSceneName;
    public string gotoSceneName;
    public List<ScenePath> scenePathList;
}
[Serializable]
public class ScenePath
{
    public string sceneName;
    public Vector2Int fromGridCeil;
    public Vector2Int gotoGridCeil;
    
    
    
}