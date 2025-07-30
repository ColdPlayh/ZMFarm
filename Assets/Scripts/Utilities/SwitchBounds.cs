using System;
using Cinemachine;
using UnityEngine;


/// <summary>
/// 利用改变碰撞体实现切换摄像机限制区域
/// </summary>
public class SwitchBounds : MonoBehaviour
{
  

   
   private void OnEnable()
   {
      //注册场景切换后事件
      //需要切换摄像机限制区域的碰撞体
      EventHandler.AfterPlayerMoveEvent += SwitchConfineShape;
   }

   
   private void OnDisable()
   {
      //注销事件
      EventHandler.AfterPlayerMoveEvent-= SwitchConfineShape;
   }

   private void SwitchConfineShape()
   {
      Debug.Log("寻找bounds");
      //利用tag寻找碰撞体 
      PolygonCollider2D confineShape =
         GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();
      //设置限制区域
      CinemachineConfiner confiner = GetComponent<CinemachineConfiner>();
      //设置限制区域
      confiner.m_BoundingShape2D = confineShape;
   }
}
