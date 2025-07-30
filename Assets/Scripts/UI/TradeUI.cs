using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LYFarm.Inventory
{
    public class TradeUI : MonoBehaviour
    {
        public Image itemIcon;
        public Text itemName;
        public InputField tardeAmount;
        public Button submitBtn;
        public Button cancelBtn;

        private ItemDetails itemDetails;

        private bool isSell;

        private void Awake()
        {
            cancelBtn.onClick.AddListener(CancelTrade);
            submitBtn.onClick.AddListener(TradeItem);
        }


        public void SetupTradeUI(ItemDetails itemDetails, bool isSell)
        {
            this.itemDetails = itemDetails;
            //设置图片和名字
            itemIcon.sprite = this.itemDetails.itemIcon;
            itemName.text = this.itemDetails.itemName;
            //获取交易状态
            this.isSell = isSell;
            //清空输入框
            tardeAmount.text = String.Empty;
        }

        public void TradeItem()
        {
            //
            var amount = Convert.ToInt32(tardeAmount.text);
            InventoryManager.Instance.TradeItem(itemDetails,amount,isSell);
            CancelTrade();
        }
        public void CancelTrade()
        {
            gameObject.SetActive(false);
        }
    }
}