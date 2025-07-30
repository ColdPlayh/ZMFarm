using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LYFarm.Inventory
{
    public class Box : MonoBehaviour
    {
        //数据模板
        public InventoryBag_SO boxInventoryTemplate;

        //当前box的数据
        public InventoryBag_SO boxInventoryData;

        public GameObject mouseIcon;
        //是否可以打开
        private bool canOpen;

        //是否已经打开
        private bool isOpen;

        public int index;

        private void OnEnable()
        {
            if (boxInventoryData is null)
            {
                boxInventoryData = Instantiate(boxInventoryTemplate);
            }
            mouseIcon.SetActive(false);

           
        }

        private void Update()
        {
            if (!isOpen && canOpen && Input.GetMouseButtonDown(1))
            {
                //打开箱子
                EventHandler.CallBaseBagOpenEvent(SlotType.Box,boxInventoryData);
                isOpen = true;
                mouseIcon.SetActive(false);
            }
            if (!canOpen && isOpen)
            {
                EventHandler.CallBaseBagCloseEvent(SlotType.Box,boxInventoryData);
                isOpen = false;
            }
            if ( isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                EventHandler.CallBaseBagCloseEvent(SlotType.Box,boxInventoryData);
                isOpen = false;
                if(canOpen)
                    mouseIcon.SetActive(true);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = true;
                mouseIcon.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = false;
                mouseIcon.SetActive(false);
            }

            
        }

        /// <summary>
        /// 初始创建的时候会执行 重新绘制的时候会执行
        /// </summary>
        /// <param name="boxIndex"></param>
        public void Init(int boxIndex)
        {
            index = boxIndex;
            //保存数据/读取数据
            var key = this.name + index;
            //重新显示箱子的时候 读取数据
            if (InventoryManager.Instance.GetBoxDictList(key) is not null)
            {
                boxInventoryData.itemList = InventoryManager.Instance.GetBoxDictList(key);
            }
            //新建箱子 保存数据
            else
            {
                InventoryManager.Instance.AddBoxData(this);
            }
        }
    }
}

