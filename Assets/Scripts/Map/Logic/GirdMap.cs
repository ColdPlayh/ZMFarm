using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

//编辑模式下运行
[ExecuteInEditMode]
public class GirdMap : MonoBehaviour
{
   public MapData_SO mapData;
   public GirdType girdType;
   public Tilemap currentTilemap;
   private void OnEnable()
   {
      if (!Application.IsPlaying(this))
      {
         currentTilemap = GetComponent<Tilemap>();
         if (mapData != null)
         {
            mapData.tileProperties.Clear();
         }
         
      }
   }

   private void OnDisable()
   {
      if (!Application.IsPlaying(this))
      {
         //获取当前的tileMap
         currentTilemap = GetComponent<Tilemap>();
         //更新地图数据
         UpdateTileProperties();
#if UNITY_EDITOR
         if (mapData != null)
         {
            EditorUtility.SetDirty(mapData);
         }
#endif
      }
   }

   private void UpdateTileProperties()
   {
      //根据瓦片地图的实际绘制方式限制最小的地图大小
      currentTilemap.CompressBounds();
      if (!Application.IsPlaying(this))
      {
         if (mapData != null)
         {
            //已绘制部分的左下角
            Vector3Int startPos = currentTilemap.cellBounds.min;
            //已绘制部分的右上角
            Vector3Int endPos = currentTilemap.cellBounds.max;
            //遍历每一个格子
            for (int x = startPos.x; x < endPos.x; x++)
            {
               for (int y = startPos.y; y < endPos.y; y++)
               {
              
                  //获取tile
                  TileBase tile = currentTilemap.GetTile(new Vector3Int(x, y, 0));
                  /*
                   * 如果Tile存在
                   * 逻辑：我们在CanDig CanDrop PlaceFurniture 和NPCObstacle设置上对应的类型（girdType）
                   * 如果我们在这个tile层绘制了东西则代表这些部分可以进行对应类型的操作
                   * 那么我们要储存的是瓦片的 位置 它对应的Type 如果绘制的部分代表可以做对应类型的操作 设置bool为true
                   */
                  if (tile != null)
                  {
                     TileProperty newTileProperty = new TileProperty
                     {
                        //位置
                        tileCoordinate = new Vector2Int(x,y),
                        //当前tile的类型
                        girdType = this.girdType,
                        //true
                        boolTypeValue = true
                     };
                     mapData.tileProperties.Add(newTileProperty);

                  }
               }
            }
         }
      }
   }
}
