using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


namespace LYFarm.Inventory
{
    public class SlotUI : MonoBehaviour,
        IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        [Header("组件")]
        [SerializeField]
        //显示的物品图片
        private Image slotImage;
        [SerializeField]
        //数量文本
        private TextMeshProUGUI amountText;
        [FormerlySerializedAs("highLight")]
        //高亮的框
        public Image slotHighLight;
        [SerializeField]
        //Button本身
        private Button selfBuuton;

        
        [Header("容器类型")] 
        public SlotType slotType;

        //是否被选中
         public bool isSelected;
        
        /// <summary>
        /// 将该格子储存的物品的详细数据缓存
        /// </summary>
        [HideInInspector]public ItemDetails itemDetails;
        /// <summary>
        /// 该格子储存的物品数量
        /// </summary>
         public int itemAmount;
        //格子序号
        public int slotIndex;

        private bool isStartDrag = false;

        /// <summary>
        /// 所有库存相关UI的父级Canvas对象
        /// </summary>
      
        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

        public InventoryLocation Location
        {
            get
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    _=>InventoryLocation.Player
                };
            }
        }
        private void Awake()
        {
            if (slotImage==null)
                slotImage = transform.GetChild(0).GetComponent<Image>();
            if (amountText == null)
                amountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            if (slotHighLight == null)
                slotHighLight = transform.GetChild(2).GetComponent<Image>();
            if (selfBuuton == null)
                selfBuuton = GetComponent<Button>();
            
        }


     
       

        private void Start()
        {
            
            isSelected = false;
            //全局变量会被初始化 所以不能用itemDetails==null
            if (itemDetails==null)
            {
                UpdateEmptySlot();
            }
        }
        /// <summary>
        /// 当前UI组件被点击后的事件
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            
            //:OPTIMIZE：物品被选中后 鼠标点击空格子或者其他位置取消选中。

            #region slot被点击后设置选中框

            if (itemDetails==null)
                return;
            
            //点击设置高亮
            isSelected = !isSelected;
            inventoryUI.UpdateSlotHighLight(slotIndex);

            #endregion
            

            #region 如果点击的物品可以被举起 将切换相关动画并举起物体

            //只有在玩家的背包中点选才会有举起的效果
            if (slotType == SlotType.Bag )
            {
                /*
                 * 1、切换玩家动画为举起同时显示举起的物品
                 * 2、
                 */
                EventHandler.CallItemSelectedEvent(itemDetails,isSelected);
            }
            
            

            #endregion
            
        }
        //拖拽开始
        public void OnBeginDrag(PointerEventData eventData)
        {
            //数量大于0的才有物品
            if (itemAmount != 0)
            {
                //显示并设置拖拽的图片
                inventoryUI.dragItem.enabled = true;
                inventoryUI.dragItem.sprite = slotImage.sprite;
                inventoryUI.dragItem.SetNativeSize();
                //设置当前物品被选中
                isSelected = true;
                inventoryUI.UpdateSlotHighLight(slotIndex);
                isStartDrag = true;
            }
            
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isStartDrag) return;
            //拖拽中 更新图片位置
            inventoryUI.dragItem.transform.position = Input.mousePosition;

        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isStartDrag) return;
            //关闭拖拽土拍你
            inventoryUI.dragItem.enabled = false;

            //显示当前拖拽结束位置的目标
            // Debug.Log(eventData.pointerCurrentRaycast.gameObject);
            //获取拖拽结束射线碰撞的obj
            GameObject currRayObj = eventData.pointerCurrentRaycast.gameObject;
            //射线出发的obj是UI
            if (currRayObj != null)
            {
                //确保该obj存在
                // if (!currRayObj.TryGetComponent(typeof(SlotUI),out var targetSlot)) 
                //     return;
                if (currRayObj.GetComponent<SlotUI>() == null)
                    return;
                var targetSlot = currRayObj.GetComponent<SlotUI>();
                int targetIndex = targetSlot.slotIndex;
                //背包和背包的交换
                if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex,targetIndex);
                    //让拖拽道德目标格子高亮
                    targetSlot.isSelected = true;
                    inventoryUI.UpdateSlotHighLight(targetIndex);
                }

                //买
                else if (slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag)
                {
                    EventHandler.CallShowTradeUI(itemDetails,false);
                    
                    
                }
                //卖
                else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop)
                {
                    EventHandler.CallShowTradeUI(itemDetails, true);
                }
                //从人物到箱子从箱子到人物
                else if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType!=targetSlot.slotType)
                {
                    Debug.Log("跨背包");
                    //跨背包交换   
                    InventoryManager.Instance.SwapItem(Location,slotIndex,targetSlot.Location,targetIndex);
                }
               
                //:HACK:临时解决“当物品被拖拽到一个空格子 当前格子仍然可以高亮”的Bug

                #region 临时解决“当物品被拖拽到一个空格子 当前格子仍然可以高亮”的Bug
                if (!selfBuuton.interactable)
                {
                    itemAmount = 0;
                    itemDetails = new ItemDetails();
                }
                #endregion
                isStartDrag = false;

            }
            //如果没有触碰到UI 既在世界地图位置松开 生成一个物体
            //TODO：垃圾桶功能

            #region 垃圾桶功能（未实现）

            // else
            // {
            //     if (!itemDetails.canDropped) return;
            //     //摄像机距离地面z轴是-10 所以需要补偿出来
            //     var pos = Camera.main.ScreenToWorldPoint(
            //         new Vector3(Input.mousePosition.x,Input.mousePosition.y,-Camera.main.transform.position.z));
            //     EventHandler.CallInstantiateItemInScene(itemDetails.itemID,pos);
            // }

            #endregion
           
        }
        /// <summary>
        /// 更新格子的UI和信息
        /// </summary>
        /// <param name="item">需要显示的item的ItemDetails</param>
        /// <param name="amount">需要显示的item的数量</param>
        public void UpdateSlot(ItemDetails item,int amount)
        {
            //缓存itemDetails数据
            itemDetails = item;
            //设置现实的图片
            slotImage.sprite = item.itemIcon;
            slotImage.enabled = true;
            //缓存库存并设置现实的text
            itemAmount = amount;
            amountText.text = amount.ToString();
            //设置按钮是否可用
            selfBuuton.interactable = true;

        }
        /// <summary>
        /// 将slot更新为空（没有物体存储的状态）
        /// </summary>
        public void UpdateEmptySlot()
        {
            if (isSelected)
            {
                isSelected = false;
                inventoryUI.UpdateSlotHighLight(-1);
                EventHandler.CallItemSelectedEvent(itemDetails,isSelected);
                
            }
            itemDetails = null;
            slotImage.enabled = false;
            amountText.text=String.Empty;
            selfBuuton.interactable = false;
        }
    }
}
