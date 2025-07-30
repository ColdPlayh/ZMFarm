using System;
using Inventory.Data_SO;
using LYFarm.CropPlant;
using LYFarm.Inventory;
using LYFarm.Map;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * 1、鼠标更新默认为无效为默认图片
 * 2、当在背包中选择物品后会设置鼠标有效（可以进行更新检测）并设置图片
 * 3、更新图片 （只有在非背包位置才会切换为normal之外的图片）
 * 4、检测鼠标是否可用（根据当前的网格）
 * 5、首先获取鼠标所处位置的世界坐标和网格坐标
 * 6、位于使用范围内可用 范围外不可用
 * 7、根据所处瓦片（是否可以扔东西、是否可以种植、是否可以放置家具、是否可以挖掘）
 *    和当前选中物品的属性（是否可仍） 和类型（种子：种植 、工具：使用、家具：摆放）
 *
 * ps：只有物品被选中后才会启用鼠标更新
 * 
 */

public class CursorManager : MonoBehaviour
{
   
   public Sprite normal, tool, seed,item;

   /// <summary>
   /// 储存当前鼠标图片
   /// </summary>
   private Sprite currentSprite;

   
   //鼠标image对象
   private Image cursorImage;
   //建造图片
   private Image buildImage;
   //当前鼠标的图片
   private RectTransform cursorCanvas;


   //主摄像机
   private Camera maincCamera;
   //当前场景的网格
   private Grid currentGrid;
   //鼠标位置的世界坐标
   private Vector3 mouseWorldPos;
   //鼠标位置的网格坐标
   private Vector3Int mouseGridPos;
   //鼠标是否有效
   private bool cursorEnable;
   //鼠标当前坐标位置是否可用
   private bool cursorPositionValid;

   //当前选中的物品
   private ItemDetails currentItem;
   private Transform playerTransform => FindObjectOfType<Player>().transform;

   #region 生命周期方法
   private void OnEnable()
   {
      //注册到物品选择的委托中
      EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
      EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
      EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
      
   }

  

   private void Start()
   {
      //获取主摄像机
      maincCamera=Camera.main;
      //获取鼠标的panel
      cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
      //如果存在这个panel
      if (cursorCanvas)
      {
         //获取鼠标Image对象
         cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
         buildImage=cursorCanvas.GetChild(1).GetComponent<Image>();
         buildImage.gameObject.SetActive(false);
      }
         
      //设置当前的图片
      currentSprite = normal;
      SetCursorImage(normal);
   }
   private void Update()
   {
      //如果没找到canvas则不更新
      if (cursorCanvas==null) return;
      
      
      //更新鼠标图片位置
      cursorImage.transform.position = Input.mousePosition;

     
      //OPTIMIZE:性能不好
      
      //如果鼠标不处于UI图片上且鼠标有效
      if (!InteractWithUI() && cursorEnable)
      {

        
           
            //更新鼠标图片
            SetCursorImage(currentSprite);
            //检测鼠标是否可用
            CheckCursorValid();
            //检测鼠标点击是否有效 并执行相关方法
            CheckPlayerInput();

         
      }
      else
      {
         //设置为默认图片
         SetCursorImage(normal);
         //关闭buildImage
         buildImage.gameObject.SetActive(false);
      }
         
   }
   private void OnDisable()
   {
      EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
      EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
      EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
     
   }
   #endregion

   #region 事件方法
   /// <summary>
   /// 当鼠标选择物品的时候调用该方法
   /// </summary>
   /// <param name="ItemDetails">点击格子物品详情</param>
   /// <param name="isSelected">当前点击的格子是否被选中</param>
   private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
   {
      Debug.Log(itemDetails+":"+isSelected);
      //如果没有被选中
      if (!isSelected)
      {
         //如果没有选中背包中的物品则没有当前选中的item
         currentItem = null;
         //设置鼠标无效 以保证在update中不需要去更新鼠标图片和检查地面
         cursorEnable = false;
         //设置为默认的鼠标图片
         currentSprite = normal;
         //取消显示         
         buildImage.gameObject.SetActive(false);
      }
      else
      {
         

         currentItem = itemDetails;
         
         //WORKFLOW:选择不同的物品返回不同的cursor图片
         //如果被选中了 则更改图片
         currentSprite = itemDetails.itemType switch
         {
            ItemType.Seed => seed,
            ItemType.Commodity => item,
            ItemType.ChopTool => tool,
            ItemType.HoeTool =>tool,
            ItemType.WaterTool =>tool,
            ItemType.BreakTool =>tool,
            ItemType.CollectTool =>tool,
            ItemType.ReapTool =>tool,
            ItemType.Furniture =>tool,
            
            _ => normal
         };
         //如果是图纸
         if (itemDetails.itemType == ItemType.Furniture)
         {
            buildImage.gameObject.SetActive(true);
            buildImage.sprite = itemDetails.itemOnWorldSprite;
            buildImage.SetNativeSize();
         }
         else
         {
            buildImage.gameObject.SetActive(false);
         }
         //当选择背包中的物品后设置鼠标有效
         cursorEnable = true;
      }
   }
   
   //加载场景前设置鼠标无效
   private void OnBeforeSceneUnloadEvent()
   {
      cursorEnable = false;
   }

   private void OnAfterSceneLoadedEvent()
   {
      currentGrid = FindObjectOfType<Grid>();
      
   }
   #endregion
   
   #region 设置鼠标样式
   /// <summary>
   /// 设置鼠标的图片
   /// </summary>
   /// <param name="sprite"></param>
   private void SetCursorImage(Sprite sprite)
   {
      
      
      cursorImage.sprite = sprite;
      cursorImage.color = new Color(1, 1, 1, 1);
   }

   /// <summary>
   /// 设置鼠标可用
   /// </summary>
   private void SetCursorValid()
   {
      cursorPositionValid = true;
      cursorImage.color = new Color(1, 1, 1, 1);
      //设置buildimage1的颜色
      if (buildImage.isActiveAndEnabled)
      {
         buildImage.color = new Color(1, 1, 1, 0.5f);
      }
   }

   private void SetCursorInValid()
   {
      cursorPositionValid = false;
      cursorImage.color = new Color(1, 0, 0, 0.4f);
      if (buildImage.isActiveAndEnabled)
      {
         buildImage.color = new Color(1, 0, 0, 0.5f);
      }
   }
   #endregion
   /// <summary>
   /// 设置鼠标是否可用
   /// <para>当我们在背包中选中物品后才会调用</para>
   /// </summary>
   private void CheckCursorValid()
   {
      //获取鼠标的世界坐标位置和所处于的网格位置
      mouseWorldPos = maincCamera.ScreenToWorldPoint(
         new Vector3(Input.mousePosition.x,Input.mousePosition.y,-maincCamera.transform.position.z));
      mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
      //人物所处于位置的网格坐标
      var playerGridPos = currentGrid.WorldToCell(playerTransform.position);

      
      //设置buildimage
      buildImage.rectTransform.position = Input.mousePosition;
      //当我们选中物品后
      if(Math.Abs(playerGridPos.x-mouseGridPos.x)>currentItem.itemUseRadius
         || Math.Abs(playerGridPos.y-mouseGridPos.y)>currentItem.itemUseRadius)
      {
         SetCursorInValid();
         return;
      }
      Debug.Log(mouseGridPos);
      //根据网格位置获得当前鼠标所处于的瓦片的地图数据
      TileDetails currentTileDetails = GirdManager.Instance.GetTileDetailsOnMouseGridPosition(mouseGridPos);
     
      //如果存在地图数据
      if (currentTileDetails != null)
      {
         CropDetails cropDetails = CropManager.Instance.GetCropDetails(currentTileDetails.seedItemID);
         Crop crop = GirdManager.Instance.GetCropObject(mouseWorldPos);
         //WORKFLOW:补全所有类型物品的判断
         //判断当前我们背包选择的物品的类型
         switch (currentItem.itemType)
         {
            case ItemType.Seed:
               //瓦片被挖掘了并且没有种上种子
               if (currentTileDetails.daysSinceDig > -1 && currentTileDetails.seedItemID == -1)
                  SetCursorValid();
               else 
                  SetCursorInValid();
               break;
            //选中的是商品
            case ItemType.Commodity:
               
               //如果当前瓦片可以仍东西 且当前物品可以仍
               if (currentItem.canDropped&&currentTileDetails.canDropItem )
               {
                  //设置鼠标可用
                  SetCursorValid();
               }
               else
               {
                  //设置鼠标不可用
                  SetCursorInValid();
               }
               
               break;
            //选中的是锄头
            case ItemType.HoeTool:
               //如果当前瓦片可以被挖掘 设置鼠标可用
               if (currentTileDetails.canDig) SetCursorValid(); 
               else SetCursorInValid();
               break;
            case ItemType.WaterTool:
               //当前瓦片已经被挖掘 且没有浇过水
               if (currentTileDetails.daysSinceDig>-1 && currentTileDetails.daysSinceWatered==-1) 
                  SetCursorValid(); 
               else SetCursorInValid();
               break;
            case ItemType.CollectTool:
            
               if (cropDetails is not null)
               {
                  if (cropDetails.CheckToolAvailable(currentItem.itemID))
                  {
                     if (currentTileDetails.growthDays >= cropDetails.TotalGrowthDays)
                        SetCursorValid();
                     else
                        SetCursorInValid();
                  }
               }
               else
               {
                  SetCursorInValid();
               }
               break;
            case ItemType.ChopTool:
            case ItemType.BreakTool:
               if (crop is not null)
               {
                  //crop可以收获 并且可以使用当前工具收获
                  if (crop.canHarvest && crop.cropDetails.CheckToolAvailable(currentItem.itemID))
                  {
                     SetCursorValid();
                  }
                  else
                  {
                     SetCursorInValid();
                  }
               }
               else
               {
                  SetCursorInValid();
               }
               break;
            case ItemType.ReapTool:
               if (GirdManager.Instance.HaveReapableItemsInRadius(currentItem,mouseWorldPos)) SetCursorValid(); 
               else SetCursorInValid();
               break;
            case ItemType.Furniture:
               buildImage.gameObject.SetActive(true);
               var bluePrint = InventoryManager.Instance.bluePrintData_So.GetBluePrintDetails(currentItem.itemID);
               bool haveFurniture = HaveFurnitureInRadius(bluePrint);
               //是否可以放置家具 并且是否有足够的资源 没有其他家具
               if (currentTileDetails.canPlaceFurniture && InventoryManager.Instance.CheckStock(currentItem.itemID)&&!haveFurniture)
               {
                  SetCursorValid();
               }
               else
               {
                  SetCursorInValid();
               }
                 
               
               break;
               
         }
      }
      //如果当前瓦片我们压根没有属性
      else
      {
         //直接设置鼠标不可用
         SetCursorInValid();
      }
   }

   private bool HaveFurnitureInRadius(BluePrintDetails bluePrintDetails)
   {
      var buidItem = bluePrintDetails.buildPrefab;
      Vector2 mousePos = mouseWorldPos;
      var size = buidItem.GetComponent<BoxCollider2D>().size;

      var other = Physics2D.OverlapBox(mousePos, size, 0);
      if(other !=null)
         return other.GetComponent<Furniture>();
      else
      {
         return false;
      }
   }

   #region 功能方法
   /// <summary>
   /// 鼠标属于的位置是否在UI中
   /// </summary>
   /// <returns></returns>
   private bool InteractWithUI()
   {
      if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
      {

         return true;
      }

      return false;

   }

   private void CheckPlayerInput()
   {
      //点击鼠标左键 且鼠标当前所在的瓦片有效（或者说当前的瓦片可以扔东西、种植、还是什么）
      if (Input.GetMouseButtonDown(0)&& cursorPositionValid)
      {
         /*
          * 鼠标点击后执行事件 mouseWorldPos：鼠标所在位置的世界坐标 currentItem：当前选中的物品的数据
          * 1、
          * 2、
          * 3、
          */
        
        EventHandler.CallMouseClickEvent(mouseWorldPos,currentItem);
      }
      
   }
   #endregion
}
