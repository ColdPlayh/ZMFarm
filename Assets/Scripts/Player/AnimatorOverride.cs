using System;
using System.Collections;
using System.Collections.Generic;
using LYFarm.Inventory;
using UnityEngine;

public class AnimatorOverride : MonoBehaviour
{
   #region 字段
   
   //存放所有部位的animator
   private Animator[] animators;
   [Header("举起的物体")]
   public SpriteRenderer holdItem;

   /// <summary>
   /// 存放主角在不同状态下每个部位对应的动画控制器
   /// </summary>
   public List<AnimatorType> animatorTypes;
   /// <summary>
   /// 存放所有animator的字典
   /// <param name="key">主角部位名称</param>
   /// <param name="value">主角部位对应的Animator组件</param>
   /// </summary>
   private Dictionary<string, Animator> animatorNameDict = new Dictionary<string, Animator>();
   
   #endregion
   #region 生命周期方法
   
   private void Awake()
   {
      //初始化动画列表
      animators = GetComponentsInChildren<Animator>();

      //初始化字典
      foreach (var animator in animators)
      {
         
         animatorNameDict.Add(animator.name,animator);
      }
      
   }
   private void OnEnable()
   {
      //注册到当玩家选中背包中可以举起的物品执行的event
      EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
      //注册到卸载当前场景需要执行的event
      EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
      EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
   }

  

   private void OnDisable()
   {
      EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
      EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
      EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
   }
   #endregion

   #region 事件方法
   
   /// <summary>
   /// 切换玩家动画为默认站立动画 隐藏举起的物体
   /// </summary>
   private void OnBeforeSceneUnloadEvent()
   {
      holdItem.enabled = false;
      SwitchAnimtor(PartType.None);
   }
   /// <summary>
   /// 玩家举起背包中的物品
   /// </summary>
   /// <param name="itemDetails">需要举起物品的详情</param>
   /// <param name="isSelected">是否被选中</param>
   private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
   {
      
      //TODO:不能被举起的物品不可以被举起
      //WORKFLOW：选中不同类型的物品切换玩家的动画
      //不同的物品类型返回不同的动画类型
      PartType currType=itemDetails.itemType switch
         
      {
         ItemType.Seed=>PartType.Carry,
         ItemType.Commodity=> PartType.Carry,
         ItemType.HoeTool=>PartType.Hoe,
         ItemType.WaterTool=>PartType.Water,
         ItemType.CollectTool=>PartType.Collect,
         ItemType.ChopTool=>PartType.Chop,
         ItemType.BreakTool=>PartType.Break,
         ItemType.ReapTool=>PartType.Reap,
         ItemType.Furniture=>PartType.Carry,
         
         _ => PartType.None
      };
      if (currType == PartType.Carry && !itemDetails.canCarried)
         currType = PartType.None;
      
      //如果未选择设置为None的动画状态
      //并且隐藏图片
      if (!isSelected)
      {
         currType = PartType.None;
         //隐藏举起的物品
         holdItem.enabled = false;
      }
      else
      {
         //显示举起的图片
         if (currType == PartType.Carry)
         {
            //切换举起物品的土拍你
            holdItem.sprite = itemDetails.itemOnWorldSprite;
            //隐藏显示的物品
            holdItem.enabled = true;
         }
         else
         {
            holdItem.enabled= false;
         }
         
      }
      //切换动画
      SwitchAnimtor(currType);
      
   }
   /// <summary>
   /// 玩家收获物品动画触发后执行的事件
   /// </summary>
   /// <param name="ID"></param>
   private void OnHarvestAtPlayerPosition(int ID)
   {
      //获取收获物品的图片
      Sprite itemSprite = InventoryManager.Instance.GetItemDetails(ID).itemOnWorldSprite;
      if (itemSprite is not null)
      {
         if (holdItem.enabled is false)
         {
            StartCoroutine(ShowItem(itemSprite));
         }
      }
   }
   #endregion

   /// <summary>
   /// 在收获的时候显示收获的物品
   /// </summary>
   /// <param name="itemSprite"></param>
   /// <returns></returns>
   private IEnumerator ShowItem(Sprite itemSprite)
   {
      holdItem.sprite = itemSprite;
      holdItem.enabled = true;
      yield return new WaitForSeconds(0.45f);
      holdItem.enabled = false;
   }
   #region 功能方法
   //切换动画
   private void SwitchAnimtor(PartType currType)
   {
      //根据当前所处的动画类型切换对应的AnimatorOverrideController
      foreach (var animatorType in animatorTypes)
      {
         if (animatorType.partType == currType)
         {
            //切换对应部位Animatory组件的运行的控制器
            animatorNameDict[animatorType.partName.ToString()].
               runtimeAnimatorController =animatorType.overrideController;
         }
      }
   }
   #endregion
}
