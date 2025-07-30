using System;
using UnityEngine;



namespace LYFarm.Inventory
{
 [RequireComponent(typeof(SlotUI))]
 public class ActionBarButton : MonoBehaviour
 {
     public KeyCode key;
     private SlotUI _slotUI;
     private bool canUse = true;

     private void Awake()
     {
         _slotUI = GetComponent<SlotUI>();
     }

     private void OnEnable()
     {
         EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
     }

   

     private void OnDisable()
     {
         EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
     }
     private void OnUpdateGameStateEvent(GameState gameState)
     {
         //游戏暂停的时候键盘输入无效
         canUse = gameState == GameState.GamePlay;
     }
     private void Update()
     {
         //获取key
         if (Input.GetKeyDown(key) && canUse)
         {
             //当前格子存在item
             if(_slotUI.itemDetails is not null)
             {
                 //设置选中状态
                 _slotUI.isSelected = !_slotUI.isSelected;
                 //设置高亮
                 if (_slotUI.isSelected)
                 {
                     _slotUI.inventoryUI.UpdateSlotHighLight(_slotUI.slotIndex);
                 }
                 else
                 {
                     _slotUI.inventoryUI.UpdateSlotHighLight(-1);
                 }
                 //呼叫事件
                 EventHandler.CallItemSelectedEvent(_slotUI.itemDetails,_slotUI.isSelected);
             }
         }
     }
 }

}
