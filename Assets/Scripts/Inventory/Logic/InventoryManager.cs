
using System;
using System.Collections.Generic;
using Inventory.Data_SO;
using LYFarm.Save;
using UnityEngine;

namespace LYFarm.Inventory
{
    /// <summary>
    /// <para>所有库存数据（玩家背包、箱子、商店等）的管理者（Controller）</para>
    /// <para>仅操作数据:通过EventHandler来进行View的更新</para>
    /// </summary>
    public class InventoryManager : Singleton<InventoryManager>,ISaveable
    {
        [Header("物品模板数据")]
        //物品数据
        public ItemDataList_SO itemDataList_So;


        [Header("建造图纸")]
        //蓝图数据
        public BluePrintData_SO bluePrintData_So;

        [Header("玩家背包数据")] 
        //玩家背包的初始模板数据
        public InventoryBag_SO playerBagTemplate_So;
        //玩家背包数据
        public InventoryBag_SO playerBag_So;

        //当前打开的box或者shop
        public InventoryBag_SO currentBoxBag;
        

        [Header("交易")]
        //玩家金钱
        public int playerMoney;

        //储存box中item的字典
        private Dictionary<string, List<InventoryItem>> boxDataDict = new Dictionary<string, List<InventoryItem>>();

        public int BoxDatDictCount => boxDataDict.Count;
        #region 生命周期方法
       
        private void OnEnable()
        {
            EventHandler.DropItemEvnet += OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void Start()
        {
            //注册
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            
            //开局先更新一下背包数据
            // EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag_So.itemList);
            
        }

        


        private void OnDisable()
         {
             EventHandler.DropItemEvnet -= OnDropItemEvent;
             EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
             EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
             EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
             EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
         }

        private void OnStartNewGameEvent(int index)
        {
            //游戏一开始初始化背包
            playerBag_So = Instantiate(playerBagTemplate_So);
            //初始化金钱
            playerMoney = Settings.playerStartMoney;
            //清空所有的箱子
            boxDataDict.Clear();
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag_So.itemList);
        }
        
        #endregion
         
         #region 事件方法

         //当打开一个非player背包的时候得到这个背包的数据
         private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO boxData)
         {
             currentBoxBag = boxData;
            
         }

         private void OnBuildFurnitureEvent(int itemID, Vector3 mousePos)
         {
             //移除图纸
             RemoveItem(itemID,1);
             //获取图纸的详情
             BluePrintDetails bluePrintDetails = bluePrintData_So.GetBluePrintDetails(itemID);
             //移除建造所需的资源
             foreach (var resouce in bluePrintDetails.resourceItems)
             {
                 //移除
                 RemoveItem(resouce.itemID,resouce.itemAmount);
             }
         }
         /// <summary>
         /// 丢弃物品的时候移除一个物品
         /// </summary>
         /// <param name="ID"></param>
         /// <param name="pos"></param>
         /// <param name="itemType"></param>
         private void OnDropItemEvent(int ID, Vector3 pos,ItemType itemType)
         {
             RemoveItem(ID,1);
         }
         /// <summary>
         /// 在背包中添加收获的物品
         /// </summary>
         /// <param name="ID"></param>
         private void OnHarvestAtPlayerPosition(int ID)
         {
             
             int index = GetItemIndexInBag(ID);
             AddItemAtIndex(ID,index,1);
             //更新背包UI
             EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag_So.itemList);
         }

         #endregion
        

         //根据itemID寻找他的具体数据
        /// <summary>
        /// 通过物品ID获得该物品的详细数据 即ItemDetails
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <returns></returns>
        public ItemDetails GetItemDetails(int ID)
        {
            return itemDataList_So.itemDetailsList.Find(i => i.itemID== ID);
        }

        public void TradeItem(ItemDetails itemDetails,int amount,bool isSell)
        {
            //金额
            int cost = itemDetails.itemPrice * amount;
            //获取index
            int index = GetItemIndexInBag(itemDetails.itemID);
            //如果出售
            if (isSell)
            {
                //背包中数量足够
                if (playerBag_So.itemList[index].itemAmount >= amount)
                {
                    //移除物品
                    RemoveItem(itemDetails.itemID, amount);
                    //修改金币
                    cost = (int) (cost * itemDetails.sellPercentage);
                    playerMoney += cost;
                }
            }
            //如果买入
            else
            {
                if (playerMoney - cost >= 0)
                {
                    if (CheckBagCapacity())
                    {
                        AddItemAtIndex(itemDetails.itemID,index,amount);   
                    }

                    playerMoney -= cost;
                }
            }
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag_So.itemList);
        }

        /// <summary>
        /// 检查图纸需要的资源是否足够
        /// </summary>
        /// <param name="ID">图纸的id</param>
        /// <returns></returns>
        public bool CheckStock(int ID)
        {
            var bluePrintDetails = bluePrintData_So.GetBluePrintDetails(ID);
            foreach (var resouceitem in bluePrintDetails.resourceItems)
            {
                var itemStock = playerBag_So.GetInventoryItem(resouceitem.itemID);
                if (itemStock.itemAmount < resouceitem.itemAmount)
                {
                    return false;
                }

            }
            return true;
        }
        #region 物品拾取相关方法
        /// <summary>
        /// 该方法将世界中的物体添加到背包中
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toDestory">是否销毁世界中的物体</param>
        public void AddItem(Item item,bool toDestory)
        {
            Debug.Log(item.itemID+":"+item.itemDeails.itemName);
            //检查背包是否已经有该物体
            var index = GetItemIndexInBag(item.itemID);
            //如果背包没有空位且没有当前物体则返回
            if (!CheckBagCapacity()&& index==-1)
            {
                return;
            }
            //将item数据添加到背包数据中

            AddItemAtIndex(item.itemID,index,1);
            
            
            if (toDestory)
            {
                Destroy(item.gameObject);
            }
            //更新UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag_So.itemList);
            
        }
        /// <summary>
        /// 查询背包是否有空位
        /// </summary>
        /// <returns>true 背包有空位</returns>
        private bool CheckBagCapacity()
        {
            for (int i = 0; i < playerBag_So.itemList.Count; i++)
            {
                if (playerBag_So.itemList[i].itemID == 0)
                {
                    return true;
                }
            }
            return false;
        }
        
        //OPTIMIZE:可以优化查找算法
        /// <summary>
        /// 检查背包中是否存在此物品
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <returns> -1 不存在此物品 | 返回物品在PlayerBag_So中itemList的序号(即在actionbar中的序号)</returns>
        private int GetItemIndexInBag(int ID)
        {
            for (int i = 0; i < playerBag_So.itemList.Count; i++)
            {
                if (playerBag_So.itemList[i].itemID == ID)
                {
                    return i;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// 为背包添加物品|更新已存在物品的数据
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <param name="index">背包格子序号</param>
        /// <param name="amount">物品数量</param>
        private void AddItemAtIndex(int ID,int index,int amount)
        {
            //背包没有物品且背包有空位
            if (index==-1)
            {
                var item = new InventoryItem {itemID = ID, itemAmount = amount};
                //OPTIMIZE:可以优化逻辑
                for (int i = 0; i < playerBag_So.itemList.Count; i++)
                {
                    if (playerBag_So.itemList[i].itemID == 0)
                    {
                        playerBag_So.itemList[i] = item;
                        break;
                    }
                }
            }
            //
            else
            {
                //OPTIMIZE:可以优化逻辑
                //新的物品数量
                int currAmout = playerBag_So.itemList[index].itemAmount + amount;
                //更改当前位置物品的数据
                var item = new InventoryItem {itemID = ID, itemAmount = currAmout};
                playerBag_So.itemList[index] = item;
            }
        }
        #endregion

        #region 拖拽交换物品数据相关方法
         /// <summary>
        /// <para>交换两个物品（数据）的index</para>
        /// <para>呼叫EventHandler更新UI</para>
        /// </summary>
        /// <param name="fromIndex">当前拖拽背包格子的index</param>
        /// <param name="targetIndex">目标背包格子的index</param>
        public void SwapItem(int fromIndex,int targetIndex)
        {
            //获取数据
            InventoryItem fromItemDetails = playerBag_So.itemList[fromIndex];
            InventoryItem targetItemDetails = playerBag_So.itemList[targetIndex];
            //QUES:?是不是可以不用判断直接交换
            if (targetItemDetails.itemAmount != 0)
            {
                playerBag_So.itemList[fromIndex] = targetItemDetails;
                playerBag_So.itemList[targetIndex] = fromItemDetails;
            }
            else
            {
                playerBag_So.itemList[targetIndex] = fromItemDetails;
                playerBag_So.itemList[fromIndex] = new InventoryItem();
            }
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag_So.itemList);
            
        }

         /// <summary>
         /// 跨背包交换物品
         /// </summary>
         /// <param name="locationFrom">起始背包位置</param>
         /// <param name="fromIndex">起始背包中物品的index</param>
         /// <param name="locationTarget">目标背包的位置</param>
         /// <param name="targetIndex">目标背包的中的index</param>
         
        public void SwapItem(InventoryLocation locationFrom, int fromIndex,InventoryLocation locationTarget ,int targetIndex)
        {
            //获取两个背包的数据列表
            var currList = GetItemList(locationFrom);
            var targetList = GetItemList(locationTarget);
            InventoryItem currItem = currList[fromIndex];
     
            if (targetIndex < targetList.Count)
            {
          
                InventoryItem targetItem = targetList[targetIndex];
                
                //交换的情况 两个格子的物品都存在 是不同的物品
                if (targetItem.itemID != 0 && currItem.itemID != targetItem.itemID)
                {
           
                    currList[fromIndex] = targetItem;
                    targetList[targetIndex] = currItem;
                }
                //堆叠物品 两个格子的物品都存在 是同一个物品
                else if (targetItem.itemID != 0 && currItem.itemID == targetItem.itemID)
                {
               
                    //更新目标格子的数量
                    targetItem.itemAmount += currItem.itemAmount;
                    targetList[targetIndex] = targetItem;
                    //清空当前背包中item1的数量
                    currList[fromIndex] = new InventoryItem();
          
                   
                }
                //当前物品存在 目标格子不存在物品 
                else
                {
                
                    targetList[targetIndex] = currItem;
                    currList[fromIndex] = new InventoryItem();
                }
            }
            //数据交换完成后 呼叫ui进行更新
            EventHandler.CallUpdateInventoryUI(locationFrom,currList);
            EventHandler.CallUpdateInventoryUI(locationTarget,targetList);
        }
        public List<InventoryItem> GetItemList(InventoryLocation inventoryLocation)
        {
            return inventoryLocation switch
            {
                InventoryLocation.Player=>playerBag_So.itemList,
                InventoryLocation.Box=> currentBoxBag.itemList,
                _=>null
            };
        }
         

        #endregion

        #region 删除物品相关方法

        /// <summary>
        /// 移除指定数量的背包物品的事件
        /// </summary>
        /// <param name="ID">物品id</param>
        /// <param name="removeAmount">需要移除的数量</param>
        private void RemoveItem(int ID, int removeAmount)
        {
            //获取该物品在背包中的indx
            var index = GetItemIndexInBag(ID);

            //如果背包中该物品数量大于要移除的数量
            if (playerBag_So.itemList[index].itemAmount > removeAmount)
            {
                //计算该物品新的数量
                int amount = playerBag_So.itemList[index].itemAmount - removeAmount;
                //创建并更新数据
                var item = new InventoryItem {itemID = ID, itemAmount = amount};
                playerBag_So.itemList[index] = item;
                
            }
            //如果相同
            else if (playerBag_So.itemList[index].itemAmount == removeAmount)
            {
                // 清空该物体的数据
                var item = new InventoryItem();
                playerBag_So.itemList[index] = item;
            }
            //更新UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag_So.itemList);
            
        }

        #endregion

        #region 箱子数据保存

        //通过key获取box1的itemlist
        public List<InventoryItem> GetBoxDictList(string key)
        {
            if (boxDataDict.ContainsKey(key))
                return boxDataDict[key];
            return null;
        }

        //添加box的itemList到列表
        public void AddBoxData(Box box)
        {
            var key = box.gameObject.name + box.index;
            if (!boxDataDict.ContainsKey(key))
            {
                boxDataDict.Add(key, box.boxInventoryData.itemList);
               Debug.Log("保存箱子"+key);
            }
            
        }

        #endregion


        public string Guid => GetComponent<DataGUID>().guid;
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            //保存金钱
            saveData.playerMoney =this.playerMoney;
            //保存玩家背包数据
            saveData.inventoryDict = new Dictionary<string, List<InventoryItem>>();
            //保存所有的箱子数据
            saveData.inventoryDict.Add(playerBag_So.name,playerBag_So.itemList);
            foreach (var boxData in boxDataDict)
            {
                saveData.inventoryDict.Add(boxData.Key,boxData.Value);
            }

            return saveData;

        }

        public void RestoreData(GameSaveData gameSaveData)
        {
            
            //读取玩家的金钱
            playerMoney = gameSaveData.playerMoney;
            //虽然后面会读取内容 但是需要先创建这个数据 
            playerBag_So = Instantiate(playerBagTemplate_So);
            //读取玩家背包的数据
            playerBag_So.itemList = gameSaveData.inventoryDict[playerBag_So.name];
            //读取箱子的数据
            foreach (var boxData in gameSaveData.inventoryDict)
            {
                //如果当前的inventorymanager中的字典存在读取的key
                if (boxDataDict.ContainsKey(boxData.Key))
                {
                    boxDataDict[boxData.Key] = boxData.Value;
                }
                
            }
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag_So.itemList);
        }
    }
}
