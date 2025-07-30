using Unity.Mathematics;
using UnityEngine;


namespace LYFarm.CropPlant
{
    /// <summary>
    /// 种子成长的管理者
    /// </summary>
    public class CropManager : Singleton<CropManager>
    {
        public CropDataList_SO cropDatas;
        public Transform cropParent;
        public Grid currentGrid;

        public Season currentSeason;

        #region 生命周期方法

        private void OnEnable()
        {
            EventHandler.PlantSeedEvent += OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
        }

        private void OnDisable()
        {
            EventHandler.PlantSeedEvent -= OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
        }

        #endregion

        #region 事件方法

        private void OnPlantSeedEvent(int seedID, TileDetails tileDetails)
        {
            CropDetails currentCrop = GetCropDetails(seedID);

            //种子成长数据存在 & 当前季节可种植 & 当前瓦片没有种植庄稼
            if (currentCrop != null && SeasonAvailable(currentCrop) && tileDetails.seedItemID == -1)
            {
                //设置瓦片种植的种子id
                tileDetails.seedItemID = seedID;
                //种植日期为0
                tileDetails.growthDays = 0;
                //显示庄稼
                DisplayCropPlant(tileDetails, currentCrop);
            }
            //如果已经存在种子
            else if (tileDetails.seedItemID != -1)
            {
                //显示庄稼
                //TODO:如果已经种植种子了需要刷新庄稼
                DisplayCropPlant(tileDetails, currentCrop);
            }
        }

        //场景加载完成后重新获取网格地图和种子的父物体
        private void OnAfterSceneLoadedEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
            cropParent = GameObject.FindWithTag("CropParent").transform;
        }

        private void OnGameDayEvent(int day, Season season)
        {
            
        }

        #endregion

        #region 功能方法

        /// <summary>
        /// 根据种子成长数据确定当前瓦片种植的种子的
        /// </summary>
        /// <param name="tileDetails">瓦片数据</param>
        /// <param name="cropDetails">种子成长数据</param>
        private void DisplayCropPlant(TileDetails tileDetails, CropDetails cropDetails)
        {
            int growthStages = cropDetails.growthDays.Length;
            int currentStage = 0;
            int dayCounter = cropDetails.TotalGrowthDays;
            //从最后一个生长阶段倒推
            for (int i = growthStages - 1; i >= 0; i--)
            {
                //如果种子的生长日期大于他的总共的生长日期则代表已经成熟
                if (tileDetails.growthDays >= dayCounter)
                {
                    //生长阶段为最后一个阶段
                    currentStage = i;
                    break;
                }

                dayCounter -= cropDetails.growthDays[i];
            }

            //获取对应阶段的prefab
            GameObject cropPrefab = cropDetails.growthPrefabs[currentStage];

            //获取对应阶段的图片
            Sprite cropSprite = cropDetails.growthSprites[currentStage];
            //获取位置
            Vector3 pos = new Vector3(tileDetails.girdX + 0.5f, tileDetails.girdY + 0.5f, 0);
            //生成庄稼
            GameObject cropInstance = Instantiate(cropPrefab, pos, quaternion.identity, cropParent);
            //设置图片
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;
            //设置作物数据和所在瓦片数据
            cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
            cropInstance.GetComponent<Crop>().tileDetails = tileDetails;
        }

        /// <summary>
        /// 通过id获取成长数据
        /// </summary>
        /// <param name="seedID">种子id</param>
        /// <returns></returns>
        public CropDetails GetCropDetails(int seedID)
        {
            return cropDatas.cropDetailsList.Find(cropDetails => cropDetails.seedItemID == seedID);
        }

        /// <summary>
        /// 判断当前季节是否可以种植
        /// </summary>
        /// <param name="cropDetails"></param>
        /// <returns></returns>
        private bool SeasonAvailable(CropDetails cropDetails)
        {
            for (int i = 0; i < cropDetails.canPlantedSeasons.Length; i++)
            {
                if (cropDetails.canPlantedSeasons[i] == currentSeason) return true;
            }

            return false;
        }

        #endregion
    }
}