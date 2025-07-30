using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace LYFarm.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("详细信息")] 
        public ItemToolTip ItemToolTip;
        [Header("拖拽图片")] 
        public Image dragItem;
        [Header("玩家背包")] 
        [SerializeField]
        //背包的UI对象
        private GameObject bagUI;

        public TextMeshProUGUI playerMoney;
        //背包是否打开
        private bool bagOpened;
        //储存所有的格子 包括物品栏和背包栏
        [SerializeField] private SlotUI[] playerSlots;

        [Header("通用背包")] 
        [SerializeField]
        private GameObject baseBag;

        public GameObject shopSlotPrefab;

        public GameObject boxSlotPrefab;

        [SerializeField] private List<SlotUI> baseBagSlots;


        [Header("交易UI")] 
        public TradeUI tradeUI;
        [Header("other")] 
        [SerializeField]
        //打开背包按钮
        private Button bagBtn;
        [HideInInspector]
        // public int lastSelectedSlotIndex=-1;

        #region 生命周期回调函数
        private void Awake()
        {
            //初始化开启背包按钮
            if (bagBtn == null)
            {
                bagBtn = transform.GetChild(1).GetChild(0).GetComponent<Button>();
            }
            bagBtn.onClick.AddListener(OpenBagUI);
            
           
        }

        private void OnEnable()
        {
            //注册更新背包ui的event
            EventHandler.updateInventoryUI += OnUpdateInventoryUI;
            //注册卸载当前场景需要执行的event
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;

            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent += OnBagCloseEvent;
            EventHandler.ShowTradeUI += OnShowTradeUI;
        }

       
        private void Start()
        {
            //初始化每个slot的id
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i].slotIndex = i;
            }
            //获取背包是否打开
            bagOpened = bagUI.activeInHierarchy;
            //更新金钱
            playerMoney.text = InventoryManager.Instance.playerMoney.ToString();
        }

        private void Update()
        {
            //按下b打开背包
            if (Input.GetKeyDown(KeyCode.B))
            {
                OpenBagUI();
            }
        }
        private void OnDisable()
        {
            EventHandler.updateInventoryUI -= OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent -= OnBagCloseEvent;
            EventHandler.ShowTradeUI -= OnShowTradeUI;
        }

       

        #endregion
        
        private void OnBeforeSceneUnloadEvent()
        {
            //取消所有的高亮
            UpdateSlotHighLight(-1);
        }

        #region npc背包相关的方法

        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            
            //TODO:还有箱子没有做
            GameObject slotPrefab = slotType switch
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box=>boxSlotPrefab,
                _=>null
            };
            
            //显示背包
            baseBag.SetActive(true);
            baseBagSlots = new List<SlotUI>();

            //生成slot并初始化列表
            for (int i = 0; i < bagData.itemList.Count; i++)
            {
                SlotUI slot = Instantiate(slotPrefab, baseBag.transform.GetChild(1)).GetComponent<SlotUI>();
                slot.slotIndex = i;
                baseBagSlots.Add(slot);
            }
            //强制刷新一下ui
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());
            if (slotType == SlotType.Shop || slotType==SlotType.Box)
            {
                bagUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(140, 0);
                bagUI.SetActive(true);
                bagOpened = true;
            }

            if (slotType == SlotType.Shop)
            {
                Debug.Log("action1"+slotType);
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
            }
            
            
            //刷新ui
            OnUpdateInventoryUI(InventoryLocation.Box,bagData.itemList);
           
            
        }

        private void OnBagCloseEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            //关闭背包和提示
            baseBag.SetActive(false);
            ItemToolTip.gameObject.SetActive(false);
            //更新高亮
            UpdateSlotHighLight(-1);

            //删除slot并清空列表
            foreach (var slot in baseBagSlots)
            {
                Destroy(slot.gameObject);
            }
            baseBagSlots.Clear();
            if (slotType == SlotType.Shop || slotType== SlotType.Box)
            {
                bagUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                bagUI.SetActive(false);
                bagOpened = false;
            }
            if (slotType == SlotType.Shop)
            {
                EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
            }
        }
        
        //显示tradeUI
        private void OnShowTradeUI(ItemDetails item, bool isSell)
        {
            tradeUI.gameObject.SetActive(true);
            tradeUI.SetupTradeUI(item,isSell);
        }
        #endregion
       

        #region 玩家背包UI相关方法
        /// <summary>
        /// 更新库存UI的事件
        /// </summary>
        /// <param name="location">更新的库存UI的类型</param>
        /// <param name="list">这一类型的数据</param>
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            //判断当前点击的格子所处的位置
            switch (location)
            {
                //玩家背包
                case InventoryLocation.Player:
                    for (int i = 0; i < playerSlots.Length; i++)
                    {
                        //使用数量 因为在交易的时候是先更新数量在删除数据
                        //当前格子物品数量大于0代表有物品
                        if (list[i].itemAmount>0) 
                        {
                            //获取当前各自物品的详情
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            //更新格子现实的图片和数量
                            playerSlots[i].UpdateSlot(item,list[i].itemAmount);
                        }
                        else
                        {
                            //如果格子中没有物品则更新为空格子
                            playerSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
                //商店和包裹是一个类型
                case InventoryLocation.Box:
                    for (int i = 0; i < baseBagSlots.Count; i++)
                    {
                        //使用数量 因为在交易的时候是先更新数量在删除数据
                        //当前格子物品数量大于0代表有物品
                        if (list[i].itemAmount>0) 
                        {
                            //获取当前格子物品的详情
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            //更新格子现实的图片和数量
                            baseBagSlots[i].UpdateSlot(item,list[i].itemAmount);
                        }
                        else
                        {
                            //如果格子中没有物品则更新为空格子
                            baseBagSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
                    
            }
            playerMoney.text = InventoryManager.Instance.playerMoney.ToString();
        }

        /// <summary>
        /// 打开背包的方法
        /// </summary>
        /// <returns></returns>
        public void OpenBagUI()
        {
            bagOpened = !bagOpened;
            bagUI.SetActive(bagOpened);
        }
        /// <summary>
        ///  //当切换在背包中点击的物体的时候让之前点击得物体不在高亮
        /// </summary>
        /// <param name="index">需要高亮的格子的index</param>
        public void UpdateSlotHighLight(int index)
        {
            //遍历所有的玩家格子
            foreach (var slot in playerSlots)
            {
                //如果格子被选中且点击的是当前格子
                if (slot.isSelected&&slot.slotIndex == index)
                {
                    //设置当前各自高亮
                    slot.slotHighLight.gameObject.SetActive(true);
                }
                else
                {
                    //设置格子不高亮
                    slot.isSelected = false;
                    slot.slotHighLight.gameObject.SetActive(false);
                }
            }

            
        }
        
        #endregion
        
        
    }
}
