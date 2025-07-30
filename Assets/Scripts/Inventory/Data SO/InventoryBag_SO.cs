using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryBag_SO",menuName = "Inventory/InventoryBag_SO")]
public class InventoryBag_SO : ScriptableObject
{
    /// <summary>
    /// 存放背包数据
    /// </summary>
    public List<InventoryItem> itemList;

    public InventoryItem GetInventoryItem(int ID)
    {
        return itemList.Find(i=>i.itemID==ID);
    }
    
}