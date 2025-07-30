using System;
using System.Collections;
using System.Collections.Generic;
using LYFarm.Map;
using UnityEngine;


namespace LYFarm.CropPlant
{
    public class CropGenerator : MonoBehaviour
    {
        #region 字段

        private Grid currGrid;
        public int seedItemID;

        public int growthDays;


        #endregion

        #region 生命周期方法

        private void Awake()
        {
            currGrid = FindObjectOfType<Grid>();
            
        }

        private void OnEnable()
        {
            EventHandler.GenerateCropEvent += OnGenerateCropEvent;
        }
        
        private void OnDisable()
        {
            EventHandler.GenerateCropEvent -= OnGenerateCropEvent;
        }

        

        #endregion

        #region 事件方法

        private void OnGenerateCropEvent()
        {
            GenerateCrop();
        }

        #endregion
        #region 功能方法

        public void GenerateCrop()
        {
            //获取网格坐标
            Vector3Int cropGridPos = currGrid.WorldToCell(transform.position);
            Debug.Log(cropGridPos+"1"+currGrid);
            if (seedItemID != 0)
            {
                //获取当前的tile
                var tile = GirdManager.Instance.GetTileDetailsOnMouseGridPosition(cropGridPos);
                //如果没有就新建一个
                if (tile is  null)
                {
                    tile = new TileDetails();
                    tile.girdX = cropGridPos.x;
                    tile.girdY = cropGridPos.y;
                }
                //更新tile数据
                tile.daysSinceWatered = -1;
                // tile.daysSinceDig = 0;
                tile.seedItemID = seedItemID;
                tile.growthDays = growthDays;
                //更新字典
                GirdManager.Instance.UpdateTileDetails(tile);
            }
        }

        #endregion
        
    }
}