using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFunction : MonoBehaviour
{
    public InventoryBag_SO shopData;

    private bool isOpen;

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    public void OpenShop()
    {
        isOpen = true;
       
        //使用basebag生成数据
        EventHandler.CallBaseBagOpenEvent(SlotType.Shop,shopData);
        // 打开背包游戏暂停
        EventHandler. CallUpdateGameStateEvent(GameState.Pause);
    }

    public void CloseShop()
    {
        isOpen = false;
        //清空数据
        EventHandler.CallBaseBagCloseEvent(SlotType.Shop,shopData);
        //切换为游戏状态
        EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
    }
}
