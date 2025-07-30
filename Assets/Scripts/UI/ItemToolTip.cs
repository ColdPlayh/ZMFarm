using System.Collections;
using System.Collections.Generic;
using Inventory.Data_SO;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LYFarm.Inventory
{
   public class ItemToolTip : MonoBehaviour
   {
      [Header("物品信息")]
      [SerializeField] private TextMeshProUGUI nameText;
      [SerializeField] private TextMeshProUGUI typeText;
      [SerializeField] private TextMeshProUGUI descriptionText;
      [SerializeField] private Text coinText;
      [SerializeField] private GameObject bottomPart;

      [Header("建造信息")] 
      public GameObject resoucePanel;
      [FormerlySerializedAs("resouceImages")] [SerializeField] 
      private Image[] resouceItems;
      
      

      /// <summary>
      /// 显示物品的详细信息
      /// </summary>
      /// <param name="itemDetails">需要显示详细信息物品的ItemDetails</param>
      /// <param name="slotType">物品处在什么容器中</param>
      public void SetupToolTip(ItemDetails itemDetails, SlotType slotType)
      {
         nameText.text = itemDetails.itemName;
         typeText.text = GetItemType(itemDetails.itemType);
         descriptionText.text = itemDetails.itemDescription;

         if (itemDetails.canSell)
         {
            bottomPart.SetActive(true);
            var price = itemDetails.itemPrice;
            if (slotType == SlotType.Bag)
            {
               price = (int) (price * itemDetails.sellPercentage);
            }

            coinText.text = price.ToString();
         }
         else
         {
            bottomPart.SetActive(false);
         }
         LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
      }

      /// <summary>
      /// 设置建造信息
      /// </summary>
      /// <param name="bluePrintDetails"></param>
      public void SetupResoucePanel(int ID)
      {
         //获取蓝图数据
         var bluePrintDetails = InventoryManager.Instance.bluePrintData_So.GetBluePrintDetails(ID);
         //根据图片ui1的数量进行限制
         for (int i = 0; i < resouceItems.Length; i++)
         {
            //根据blueprintdetails需要的资源数量进行初始化
            if (i < bluePrintDetails.resourceItems.Length)
            {
               var item = bluePrintDetails.resourceItems[i];
               //显示图片
               resouceItems[i].gameObject.SetActive(true);
               //设置图片
               resouceItems[i].sprite = InventoryManager.Instance.GetItemDetails(item.itemID).itemIcon;
               resouceItems[i].transform.GetChild(0).GetComponent<Text>().text = item.itemAmount.ToString();
            }
            //不显示图片
            else
            {
               resouceItems[i].gameObject.SetActive(false);
            }
         }
      }

      private string GetItemType(ItemType itemType)
      {
         return itemType switch
         {
            ItemType.Seed => "种子",
            ItemType.Commodity =>"商品",
            ItemType.Furniture => "家具",
            ItemType.BreakTool => "挖矿工具",
            ItemType.ChopTool=>"砍树工具",
            ItemType.CollectTool => "收割工具",
            ItemType.HoeTool =>"耕地工具",
            ItemType.ReapTool=>"采集工具",
            ItemType.WaterTool =>"浇灌工具",
            _ =>"杂物"
         };
      }
   }
}
