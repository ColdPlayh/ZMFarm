using System;
using System.Collections.Generic;
using System.ComponentModel;
using Inventory.Data_SO;
using LYFarm.Save;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;


namespace LYFarm.Inventory
{
    /// <summary>
    /// 生成游戏中可以拾取的item
    /// </summary>
    public class ItemMananger:MonoBehaviour,ISaveable
    {
        /// <summary>
        /// 所有在世界中生成物体 的父物体
        /// </summary>
        public Item itemPrefab;

        /// <summary>
        /// 扔出去的物品的prefab
        /// </summary>
        public Item bouncePrefab;

        public Transform playerTrans => FindObjectOfType<Player>().transform;

        /// <summary>
        /// 所有的instance和世界中生成的物品都都在这个父物体下
        /// </summary>
        private Transform itemParent;

        /// <summary>
        /// 储存每个场景中需要生成的Item
        /// <param name="key">场景名称的字符串</param>
        /// <param name="value">场景中所有item的列表</param>
        /// </summary>
        private Dictionary<string, List<SceneItem>> sceneItemDictionary=new Dictionary<string, List<SceneItem>>();

        /// <summary>
        /// 场中的家具
        /// </summary>
        private Dictionary<string, List<SceneFurniture>> sceneFurnitureDictionary =
            new Dictionary<string, List<SceneFurniture>>();

        private void Start()
        {
            //注册
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        private void OnEnable()
        {
            //注册在世界中需要生成掉落物时需要执行的事件
            EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;
            //卸载场景前需要执行的事件
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            //TIP:本身应该为afterSceneLoadedEvent
            //加载完新场景后需要执行的事件
            EventHandler.AfterPlayerMoveEvent += OnAfterSceneLoadedEvent;
            EventHandler.DropItemEvnet += OnDropItemEvent;
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }
        
        private void OnDisable()
        {
            EventHandler.InstantiateItemInScene -= OnInstantiateItemInScene;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            
            EventHandler.AfterPlayerMoveEvent -= OnAfterSceneLoadedEvent;
            EventHandler.DropItemEvnet -= OnDropItemEvent;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }
        #region 事件方法
        
        private void OnStartNewGameEvent(int obj)
        {
            //清空两个字典
            sceneItemDictionary.Clear();
            sceneFurnitureDictionary.Clear();
        }

        private void OnBuildFurnitureEvent(int itemID,Vector3 mousePos)
        {
            BluePrintDetails bluePrintDetails = InventoryManager.Instance.bluePrintData_So.GetBluePrintDetails(itemID);
            var buildItem = 
                Instantiate(bluePrintDetails.buildPrefab, mousePos,quaternion.identity,itemParent);
            //生成箱子之后 初始化他的index并且保存到字典中
            if (buildItem.GetComponent<Box>())
            {
                buildItem.GetComponent<Box>().index = InventoryManager.Instance.BoxDatDictCount;
                buildItem.GetComponent<Box>().Init(buildItem.GetComponent<Box>().index);
            }
        }


        
        /// <summary>
        /// 当前场景卸载前会执行此方法
        /// </summary>
        private void OnBeforeSceneUnloadEvent()
        {
            GetAllSceneItems();
            GetSceneAllFurniture();
        }
        
        /// <summary>
        /// 场景重新加载后需要会执行此方法
        /// <param> 获取所有在世界中生成物体的父物体</param>
        /// </summary>
        private void OnAfterSceneLoadedEvent()
        {
            //获取所有在世界生成物体的父物体
            itemParent = GameObject.FindWithTag("ItemParent").transform;
            RecreateAllItem();
            RecreateAllFurniture();
        }
        
        /// <summary>
        /// 相应EventHanlder的事件 在世界中生成对应id的物体在对应位置
        /// </summary>
        /// <param name="ID">物品id</param>
        /// <param name="pos">物品世界坐标</param>
        private void OnInstantiateItemInScene(int ID, Vector3 pos)
        {
            var item = Instantiate(bouncePrefab,pos,Quaternion.identity,itemParent);
            item.itemID = ID;
            item.GetComponent<ItemBounce>().InitBounceItem(pos,Vector2.up);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID">物品id</param>
        /// <param name="mouseWorldPos">鼠标所处位置的世界坐标</param>
        private void OnDropItemEvent(int ID, Vector3 mouseWorldPos,ItemType itemType)
        {

            if (itemType == ItemType.Seed) return;
            var item = Instantiate(bouncePrefab, playerTrans.position, Quaternion.identity, itemParent);
            item.itemID = ID;
            var dir = (mouseWorldPos - playerTrans.transform.position).normalized;
            item.GetComponent<ItemBounce>().InitBounceItem(mouseWorldPos,dir);
        }
        #endregion
        
        /// <summary>
        /// 获取当前场景的所有item
        /// </summary>
        private void GetAllSceneItems()
        {
            //储存当前场景中生成的物体
            List<SceneItem> currentSceneItems = new List<SceneItem>();
            foreach (var item in FindObjectsOfType<Item>())
            {
                //初始化每个数据
                SceneItem sceneItem = new SceneItem
                    {
                        itemID = item.itemID, 
                        position = new SerializableVector3(item.transform.position)
                    };
                //初始化列表
                currentSceneItems.Add(sceneItem);
                
            }

            //如果字典中存在当前场景则更新这个value
            if (sceneItemDictionary.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneItemDictionary[SceneManager.GetActiveScene().name] = currentSceneItems;
            }
            //如果字典中不存在则添加这个键值对
            else
            {
                sceneItemDictionary.Add(SceneManager.GetActiveScene().name,currentSceneItems);
            }

        }

        /// <summary>
        /// 获取当前场景的furniture
        /// </summary>
        public void GetSceneAllFurniture()
        {
            //初始化列表
            List<SceneFurniture> currentSceneFurnitures = new List<SceneFurniture>();
            //所有的家具
            foreach (var furniture in FindObjectsOfType<Furniture>())
            {
                //初始化家具的数据
                SceneFurniture sceneFurniture = new SceneFurniture
                {
                    furnitureID = furniture.furnitureID,
                    position =  new SerializableVector3(furniture.transform.position)
                };
                if (furniture.GetComponent<Box>())
                {
                    sceneFurniture.boxIndex = furniture.GetComponent<Box>().index;
                }
                currentSceneFurnitures.Add(sceneFurniture);
            }
            
            //更新字典
            if (sceneFurnitureDictionary.ContainsKey(SceneManager.GetActiveScene().name))
            {
                
                sceneFurnitureDictionary[SceneManager.GetActiveScene().name] = currentSceneFurnitures;
            }
            else
            {
                sceneFurnitureDictionary.Add(SceneManager.GetActiveScene().name,currentSceneFurnitures);
            }
            
        }

        /// <summary>
        /// 重新生成当前激活场景的物体
        /// </summary>
        private void RecreateAllItem()
        {
            List<SceneItem> currentSceneItems = new List<SceneItem>();
            //如果当前激活场景存在物品列表
            if (sceneItemDictionary.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneItems))
            {
                //切换场景后删除所有原有的item
                if (currentSceneItems != null)
                {
                    foreach (var item in FindObjectsOfType<Item>())
                    {
                        Destroy(item.gameObject);
                    }
                }

                foreach (var item in currentSceneItems)
                {
                    //生成物体
                    Item newItem = 
                        Instantiate(itemPrefab, item.position.ToVector3(),Quaternion.identity,itemParent);
                    //初始化id
                    newItem.Init(item.itemID);
                }
            }
        }

        private void RecreateAllFurniture()
        {
            List<SceneFurniture> currSceneFurnitures = new List<SceneFurniture>();
            if (sceneFurnitureDictionary.TryGetValue(SceneManager.GetActiveScene().name, out currSceneFurnitures))
            {
                if (currSceneFurnitures==null)
                    return;
                foreach (var furniture in currSceneFurnitures)
                {
                    BluePrintDetails bluePrintDetails = InventoryManager.Instance.bluePrintData_So.GetBluePrintDetails(furniture.furnitureID);
                    var buildItem = 
                        Instantiate(bluePrintDetails.buildPrefab, furniture.position.ToVector3(),quaternion.identity,itemParent);
                    //生成箱子之后 初始化他的index并且保存到字典中
                    if (buildItem.GetComponent<Box>())
                    {
                        buildItem.GetComponent<Box>().Init(furniture.boxIndex);
                    }
                }
            }
        }

        public string Guid => GetComponent<DataGUID>().guid;
        public GameSaveData GenerateSaveData()
        {
            //先更新一下字典 因为一般请胯下只有在切换场景之前才会获取
            GetAllSceneItems();
            GetSceneAllFurniture();
            GameSaveData saveData = new GameSaveData();
            //设置数据
            saveData.sceneItemDict = this.sceneItemDictionary;
            saveData.sceneFurnitureDict = this.sceneFurnitureDictionary;
            return saveData;

        }

        public void RestoreData(GameSaveData gameSaveData)
        {
            sceneItemDictionary = gameSaveData.sceneItemDict;
            sceneFurnitureDictionary = gameSaveData.sceneFurnitureDict;
            RecreateAllItem();
            RecreateAllFurniture();
        }
    }
}

