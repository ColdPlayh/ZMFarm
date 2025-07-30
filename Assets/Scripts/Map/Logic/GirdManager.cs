

#define TEST
using System.Collections.Generic;
using LYFarm.CropPlant;
using LYFarm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;


namespace LYFarm.Map
{
    /// <summary>
    /// 管理地图信息的类
    /// </summary>
    public class GirdManager : Singleton<GirdManager>,ISaveable
    {
        [Header("规则瓦片")] 
        //挖过的地面的瓦片
        public RuleTile digRuleTile;
        //教过水的地面的瓦片
        public RuleTile waterRuleTile;
        [Header("瓦片属性信息")] public List<MapData_SO> mapDataList;
        
        //挖坑的TileMap
        private Tilemap digTilemap;
        //浇水的TileMap
        private Tilemap waterTilemap;
        //当前的季节
        private Season currentSeason;
        
        /// <summary>
        /// 保存每个瓦片数据的字典
        /// <para>key:场景名称:坐标</para>
        /// <para>value:该瓦片的数据</para>>
        /// </summary>
        private Dictionary<string, TileDetails> tileDetailsDict = new Dictionary<string, TileDetails>();

        /// <summary>
        /// 存放所有场景是否第一次加载
        /// </summary>
        private Dictionary<string, bool> SceneIsFristLoadDict = new Dictionary<string, bool>();

        /// <summary>
        /// 存放鼠标点击位置的所有杂草
        /// </summary>
        private List<ReapItem> itemsInRadius;
        //当前的地图
        private Grid currentGrid;
        
        #region 生命周期方法

        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimationEvent += OnExecuteActionAfterAnimationEvent;
            EventHandler.AfterPlayerMoveEvent += OnAfterPlayerMoveEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
            EventHandler.RefreshCurrentMap += RefresMap;
        }

      

        private void Start()
        {
            
            //注册
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            
            foreach (var vMapData in mapDataList)
            {
                //设置每一个场景都是第一次加载
                SceneIsFristLoadDict.Add(vMapData.SceneName,true);
                //初始化每一张地图存放瓦片数据的字典
                InitTileDetailsDict(vMapData);
            }
        }

       
        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimationEvent -= OnExecuteActionAfterAnimationEvent;
            EventHandler.AfterPlayerMoveEvent -= OnAfterPlayerMoveEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
            EventHandler.RefreshCurrentMap -= RefresMap;
        }

       

        #endregion
        
        #region 事件方法

        /// <summary>
        /// 在地图上实现实际工具使用或种植 仍物品物品等相关功能
        /// </summary>
        /// <param name="mouseWorldPos"></param>
        /// <param name="itemDetails">选择的背包中的物品的数据</param>
        private void OnExecuteActionAfterAnimationEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            var mouseGridPos = currentGrid.WorldToCell(mouseWorldPos); 
            TileDetails currentTile = GetTileDetailsOnMouseGridPosition(mouseGridPos);
            if (currentTile != null)
            {
                Crop currentCrop = GetCropObject(mouseWorldPos);
                //WORKFLOW:物品使用的实际功能
                switch (itemDetails.itemType)
                {
                    //选中商品可以交易或者扔掉
                    case ItemType.Commodity:
                        // Debug.Log(222);
                        //仍物品
                        EventHandler.CallDropItemEvnet(itemDetails.itemID,mouseWorldPos,ItemType.Commodity);
                        break;
                    //选中锄头可以锄地
                    case ItemType.HoeTool:
                        SetDigGround(currentTile);
                        //距离挖地已经0天
                        currentTile.daysSinceDig = 0;
                        //不可以在被挖
                        currentTile.canDig = false;
                        //被挖过的地面不可以扔东西
                        currentTile.canDropItem = false;
                        //TODO  音效
                        EventHandler.CallPlaySoundEvent(SoundName.Hoe);
                        break;
                    //选中水壶可以浇水
                    case ItemType.WaterTool:
                        setWaterGround(currentTile);
                        currentTile.daysSinceWatered = 0;
                        EventHandler.CallPlaySoundEvent(SoundName.Water);
                        break;
                    //选中种子可以种植
                    case ItemType.Seed:
                        //种植种子的事件
                        EventHandler.CallPlantSeedEvent(itemDetails.itemID,currentTile);
                        //减少一个种子
                        EventHandler.CallDropItemEvnet(itemDetails.itemID,mouseGridPos,ItemType.Seed);
                        //播放音效
                        EventHandler.CallPlaySoundEvent(SoundName.Plant);
                        break;
                    case ItemType.CollectTool:
                        
                        if (currentCrop is not null)
                        {
                            //收割动画开始后执行的逻辑
                            currentCrop.ProcessToolAction(itemDetails,currentTile);
                            
                        }
                        break;
                    case ItemType.ChopTool:
                    case ItemType.BreakTool:
                        if (currentCrop is not null)
                        {
                            currentCrop.ProcessToolAction(itemDetails,currentCrop.tileDetails);
                        }
                        break;
                    case ItemType.ReapTool:
                        int reapCount = 0;
                        for (int i = 0; i < itemsInRadius.Count; i++)
                        {
                            
                            EventHandler.CallParticelEffectEvent
                                (ParticleEffectType.ReapableScenery,itemsInRadius[i].transform.position+Vector3.up);
                            itemsInRadius[i].SpawnHarvestItem();
                            Destroy(itemsInRadius[i].gameObject);
                            reapCount++;
                            if(reapCount>Settings.reapAmount) break;
                        }
                        EventHandler.CallPlaySoundEvent(SoundName.Reap);
                        break;
                    case ItemType.Furniture:
                        EventHandler.CallBuildFurnitureEvent(itemDetails.itemID,mouseWorldPos);
                        break;
                }
                //更新当前瓦片的数据
                UpdateTileDetails(currentTile);
            }
            
        }
        private void OnAfterPlayerMoveEvent()
        {
            //获取当前的网格
            currentGrid = FindObjectOfType<Grid>();
            //获取Tilemap中的dig层
            digTilemap = GameObject.FindWithTag("DigTile").GetComponent<Tilemap>();
            //获取Tilemap的water层
            waterTilemap = GameObject.FindWithTag("WaterTile").GetComponent<Tilemap>();
            if (SceneIsFristLoadDict[SceneManager.GetActiveScene().name])
            {
                //刷新地图之前先让石头 树木 稻草等先生成 以防再刷新地图的时候被清空
                EventHandler.CallGenerateCropEvent();
                SceneIsFristLoadDict[SceneManager.GetActiveScene().name] = false;
            }
                
            //切换地图后刷新地图
            RefresMap();
        }
        private void OnAfterSceneLoadedEvent()
        {
            
        }
    

       
        
        private void OnGameDayEvent(int day, Season season )
        {
            currentSeason = season;
            foreach (var tile in tileDetailsDict)
            {
                TileDetails tileDetails = tile.Value;
                //每过一天就需要浇一次水
                if (tileDetails.daysSinceWatered > -1)
                {
                    tileDetails.daysSinceWatered = -1;
                }


                if(tileDetails.daysSinceDig>-1)
                    tileDetails.daysSinceDig++;
                //当挖坑之后如果没有种子 5天之后就变回没有挖坑的状态
                if (tileDetails.daysSinceDig > 5 && tileDetails.seedItemID == -1)
                {
                    tileDetails.daysSinceDig = -1;
                    tileDetails.growthDays = 0;
                    tileDetails.canDig = true;
                }

                if (tileDetails.seedItemID != -1)
                {
                    tileDetails.growthDays++;
                }
                    
                
            }
            RefresMap();
        }
        #endregion
        
        #region 功能方法

        /// <summary>
        /// 获取鼠标点击位置是否存在crop
        /// </summary>
        /// <param name="mouseWorldPos">鼠标世界坐标</param>
        /// <returns></returns>
        public Crop GetCropObject(Vector3 mouseWorldPos)
        {
            Crop currentCrop = null;
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);
            for (int i = 0; i < colliders.Length; i++)
            {
                // if (colliders[i].GetComponent<Crop>())
                // {
                //     currentCrop = colliders[i].GetComponent<Crop>();
                // }
                if (colliders[i].TryGetComponent(out currentCrop))
                {
                    break;
                }
                 
            }
            Debug.Log("2"+currentCrop);
            return currentCrop;
        }

        /// <summary>
        /// 检测鼠标位置是否有杂草 
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public bool HaveReapableItemsInRadius(ItemDetails tool,Vector3 mouseWorldPos)
        {
            itemsInRadius = new List<ReapItem>();
            Collider2D[] colliders = new Collider2D[20];

            Physics2D.OverlapCircleNonAlloc(mouseWorldPos, tool.itemUseRadius, colliders);
            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] is not null)
                    {
                        if (colliders[i].GetComponent<ReapItem>())
                        {
                            var item = colliders[i].GetComponent<ReapItem>();
                            itemsInRadius.Add(item);
                        }
                    }
                }
            }

            return itemsInRadius.Count > 0;
        }
        /// <summary>
        /// 初始化存放瓦片数据字典的方法
        /// </summary>
        /// <param name="mapData">瓦片属性</param>
        private void InitTileDetailsDict(MapData_SO mapData)
        {
            foreach (TileProperty tileProperty in mapData.tileProperties)
            {
                //初始化数据
                TileDetails tileDetails = new TileDetails
                {
                    girdX = tileProperty.tileCoordinate.x,
                    girdY = tileProperty.tileCoordinate.y,
                };
                string key = tileDetails.girdX + "x" + tileDetails.girdY + "y" + mapData.SceneName;
                //已经储存过该格子的数据
                if (GetTileDetails(key) != null)
                {
                    //对已经储存过的数据进行更新
                    tileDetails = GetTileDetails(key);
                }

                //初始化属性
                switch (tileProperty.girdType)
                {
                    case GirdType.Diggable:
                        tileDetails.canDig = tileProperty.boolTypeValue;
                        break;
                    case GirdType.DropItem:
                        tileDetails.canDropItem = tileProperty.boolTypeValue;
                        break;
                    case GirdType.PlaceFurniture:
                        tileDetails.canPlaceFurniture = tileProperty.boolTypeValue;
                        break;
                    case GirdType.NPCObstacle:
                        tileDetails.isNpcObstacle = tileProperty.boolTypeValue;
                        break;
                }

                //存在的话更新新的数据
                if (GetTileDetails(key) != null)
                {
                    tileDetailsDict[key] = tileDetails;
                }
                //否则添加这个数据
                else
                {
                    tileDetailsDict.Add(key,tileDetails);
                }
                
            }
        }

        /// <summary>
        /// 使用字典获取某个瓦片的数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TileDetails GetTileDetails(string key)
        {
            if (tileDetailsDict.ContainsKey(key))
            {
                return tileDetailsDict[key];
            }
            else
            {
                return null;
            }
        }

        //通过网格坐标返回当前瓦片的数据
        public TileDetails GetTileDetailsOnMouseGridPosition(Vector3Int mouseGridPos )
        {
            string key = mouseGridPos.x + "x" + mouseGridPos.y + "y" + SceneManager.GetActiveScene().name;
            Debug.Log(key);
            return GetTileDetails(key);
        }

        /// <summary>
        /// 设置挖过的瓦片
        /// </summary>
        /// <param name="tile">要设置的瓦片的数据</param>
        private void SetDigGround(TileDetails tile)
        {
            //获取瓦片的坐标
            Vector3Int pos = new Vector3Int(tile.girdX, tile.girdY, 0);
            if (digTilemap is not null)
            {
                //改变为挖过的瓦片
                digTilemap.SetTile(pos,digRuleTile);
            }
        }
        /// <summary>
        /// 设置浇过水的瓦片
        /// </summary>
        /// <param name="tile">要设置的瓦片的数据</param>
        private void setWaterGround(TileDetails tile)
        {
            //获取瓦片的坐标
            Vector3Int pos = new Vector3Int(tile.girdX, tile.girdY, 0);
            if (digTilemap != null)
            {
                //改变为浇水的瓦片
                waterTilemap.SetTile(pos,waterRuleTile);
            }
        }
        /// <summary>
        ///  更新字典中的瓦片数据
        /// </summary>
        /// <param name="tileDetails">需要更新的瓦片的数据</param>
       
        public void UpdateTileDetails(TileDetails tileDetails)
        {
            string key=tileDetails.girdX+"x"+tileDetails.girdY+"y"+SceneManager.GetActiveScene().name;
            //如果字典中存在这个tile 更新数据
            if (tileDetailsDict.ContainsKey(key))
            {
                tileDetailsDict[key] = tileDetails;
            }
            //没有存在就添加一个键值对
            else
            {
                tileDetailsDict.Add(key,tileDetails);
            }
        }

        private void RefresMap()
        {
            //清空地图
            if (digTilemap != null)
                digTilemap.ClearAllTiles();
            if(waterTilemap!=null)
                waterTilemap.ClearAllTiles();
            foreach (var crop in FindObjectsOfType<Crop>())
            {
                Destroy(crop.gameObject);
            }
            //更新瓦片数据
            DisplayMap(SceneManager.GetActiveScene().name);
        }
        

        /// <summary>
        /// 根据瓦片数据构建地图
        /// </summary>
        /// <param name="SceneName">当前地图场景的名称</param>
        public void DisplayMap(string SceneName)
        {
            foreach(var tile in tileDetailsDict)
            {
                var key = tile.Key;
                TileDetails tileDetails = tile.Value;

                if (key.Contains(SceneName))
                {
                    if (tileDetails.daysSinceDig>-1)
                        SetDigGround(tileDetails);
                    if (tileDetails.daysSinceWatered > -1)
                        setWaterGround(tileDetails);
                    //TODO 更新种子和植物的逻辑
                    if (tileDetails.seedItemID > -1)
                    {
                        EventHandler.CallPlantSeedEvent(tileDetails.seedItemID,tileDetails);
                    }
                }
                
            }
        }

        /// <summary>
        /// 获得场景地图的范围和起始坐标（左下角）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="gridDimensions">该场景地图的范围</param>
        /// <param name="gridOriginal">该场景地图的起始坐标</param>
        /// <returns></returns>
        public bool GetGridDimensions(string sceneName, out Vector2Int gridDimensions,out Vector2Int gridOriginal)
        {
            gridDimensions = Vector2Int.zero;
            gridOriginal=Vector2Int.zero;
            foreach (var mapData in mapDataList)
            {
                if (mapData.SceneName == sceneName)
                {
                    gridDimensions.x = mapData.gridWidth;
                    gridDimensions.y = mapData.gridHeight;

                    gridOriginal.x = mapData.originalX;
                    gridOriginal.y = mapData.originalY;
                    return true;
                }
            }

            return false;
        }
        #endregion

        public void SetTestTile(Vector3Int pos)
        {
            
        }

        public string Guid => GetComponent<DataGUID>().guid;
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.tileDetailsDict = this.tileDetailsDict;
            saveData.SceneIsFristLoadDict = this.SceneIsFristLoadDict;
            return saveData;
        }

        public void RestoreData(GameSaveData gameSaveData)
        {
            tileDetailsDict = gameSaveData.tileDetailsDict;
            SceneIsFristLoadDict = gameSaveData.SceneIsFristLoadDict;
        }
    }
}

