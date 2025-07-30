using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataList_SO",menuName = "Inventory/ItemDataList_SO",order = 0)]
public class ItemDataList_SO : ScriptableObject
{
   /// <summary>
   /// 所有的Item的数据
   /// </summary>
    public List<ItemDetails> itemDetailsList;
}
