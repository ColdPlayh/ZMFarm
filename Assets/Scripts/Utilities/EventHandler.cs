using System;
using System.Collections;
using System.Collections.Generic;
using Audio.Data;
using LYFarm.Dialogue;
using UnityEngine;

public  static class EventHandler
{
    public static event Action<InventoryLocation,List<InventoryItem>> updateInventoryUI ;

    /// <summary>
    /// 执行注册到更新库存UI信息委托（updateInventoryUI）上的所有事件
    /// </summary>
    /// <param name="location">更新的是那个位置的库存UI</param>
    /// <param name="list">该类型库存所存在的数据（InventoryItem）的列表</param>
    public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
    {
        updateInventoryUI?.Invoke(location,list);
    }

    public static event Action<int, Vector3> InstantiateItemInScene;
    
    /// <summary>
    /// 执行注册方法的所有事件，在世界坐标中生成物体（item）
    /// </summary>
    /// <param name="targetItemID">生成物品的ID</param>
    /// <param name="targetPos">生成物体的位置</param>

    public static void CallInstantiateItemInScene(int targetItemID, Vector3 targetPos)
    {
        InstantiateItemInScene?.Invoke(targetItemID, targetPos);
    }


    public static event Action<int,Vector3,ItemType> DropItemEvnet;
    
    public static void CallDropItemEvnet(int ID, Vector3 pos,ItemType itemType)
    {
        DropItemEvnet?.Invoke(ID, pos,itemType);
    }

    
    public static event Action<ItemDetails, bool> ItemSelectedEvent;

    /// <summary>
    /// 当物品被选中后 判断是否举起物体 
    /// </summary>
    /// <param name="itemDetails">举起物体的itemDetails</param>
    /// <param name="isSelected">slot是否被选中</param>
    public static void CallItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        ItemSelectedEvent?.Invoke(itemDetails,isSelected);
    }

    /// <summary>
    /// 游戏分钟发生变化的时候调用的事件
    /// </summary>
    public static event Action<int, int,int,Season> GameMinuteEvent;

    /// <summary>
    /// 游戏分钟发生变化后调用
    /// <para>1.更新时间UI</para>
    /// </summary>
    /// <param name="minute">分钟</param>
    /// <param name="hour">小时</param>
    public static void CallGameMinuteEvent(int minute, int hour,int day,Season season)
    {
        GameMinuteEvent?.Invoke(minute, hour,day,season);
    }

    /// <summary>
    /// 游戏小时发生变化调用的事件
    /// </summary>
    public static event Action<int, int, int, int, Season> GameDateEvent;

    /// <summary>
    /// 游戏小时发生变化后调用
    /// <para>1.更新时间UI</para>
    /// <para></para>
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="day"></param>
    /// <param name="month"></param>
    /// <param name="year"></param>
    /// <param name="season"></param>
    public static void CallGameDateEvent(int hour, int day, int month, int year, Season season)
    {
        GameDateEvent?.Invoke(hour, day, month, year, season);
    }

    /// <summary>
    /// 在游戏日期发生变化时调用的事件
    /// </summary>
    public static event Action<int, Season> GameDayEvent;
    
    /// <summary>
    /// 游戏日期发生变化后调用
    /// <para>1.更新瓦片数据</para>
    /// </summary>
    /// <param name="day">日期</param>
    /// <param name="season">季节</param>
    public static void CallGameDayEvent(int day, Season season)
    {
        GameDayEvent?.Invoke(day, season);
    }
    /// <summary>
    /// 切换场景的Event
    /// </summary>
    public static event Action<string, Vector3> TransitionEvent;


    public static void CallTransitionEvent(string sceneName, Vector3 positionToGo)
    {
        TransitionEvent?.Invoke(sceneName, positionToGo);
    }

    /// <summary>
    /// 场景卸载之前执行此Event
    /// </summary>
    public static event Action BeforeSceneUnloadEvent;

    public static void CallBeforeSceneUnloadEvent()
    {
        BeforeSceneUnloadEvent?.Invoke();
    }

    public static event Action AfterPlayerMoveEvent;
    
    public static void CallAfterPlayerMoveEvent()
    {
        AfterPlayerMoveEvent?.Invoke();
    }
    
    /// <summary>
    /// 场景加载完成之后要执行此Event
    /// </summary>
    public static event Action AfterSceneLoadedEvent;

    public static void CallAfterSceneLoadedEvent()
    {
        AfterSceneLoadedEvent?.Invoke();
    }

    /// <summary>
    /// 切换地图后移动玩家位置的event
    /// </summary>
    public static event Action<Vector3> MovePlayerToPosition;

    /// <summary>
    /// 移动玩家位置
    /// </summary>
    /// <param name="targetPosition">玩家要移动的目标位置</param>
    public static void CallMovePlayerToPosition(Vector3 targetPosition)
    {
        MovePlayerToPosition?.Invoke(targetPosition);
    }

    /// <summary>
    /// 鼠标点击有效的瓦片后执行
    /// </summary>
    public static event Action<Vector3, ItemDetails> MouseClickEvent;

    /// <summary>
    /// 鼠标点击有效的瓦片后执行
    /// <para>1.设置玩家不可以移动</para>
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="itemDetails"></param>
    public static void CallMouseClickEvent(Vector3 pos,ItemDetails itemDetails)
    {
        MouseClickEvent?.Invoke(pos,itemDetails);
    }
    /// <summary>
    /// 主角完成一个动作的动画结束后执行的事件
    /// </summary>
    public static event Action<Vector3, ItemDetails> ExecuteActionAfterAnimationEvent;

    /// <summary>
    /// 主角完成一个动作的动画结束后执行
    /// <para>1.执行种植丢弃等动作后的逻辑</para>>
    /// </summary>
    public static void CallExecuteActionAfterAnimation(Vector3 pos,ItemDetails itemDetails)
    {
        ExecuteActionAfterAnimationEvent?.Invoke(pos,itemDetails);
    }

    /// <summary>
    /// 种植种子的事件
    /// </summary>
    public static event Action<int, TileDetails> PlantSeedEvent;

    /// <summary>
    /// 种植种子需要执行的逻辑
    /// <para>1.</para>>
    /// </summary>
    /// <param name="seedID">种子id</param>
    /// <param name="tileDetails">种植的瓦片的数据</param>
    public static void CallPlantSeedEvent(int seedID, TileDetails tileDetails)
    {
        PlantSeedEvent?.Invoke(seedID,tileDetails);
    }

    /// <summary>
    /// 收割后生成在主角背包中的事件
    /// </summary>
    public static event Action<int> HarvestAtPlayerPosition;

    public static void CallHarvestAtPlayerPosition(int ID)
    {
        HarvestAtPlayerPosition?.Invoke(ID);
    }

    /// <summary>
    /// 刷新地图的事件
    /// </summary>
    public static event Action RefreshCurrentMap;


    public static void CallRefreshCurrentMap()
    {
        RefreshCurrentMap?.Invoke();
    }

    /// <summary>
    /// 种子收割时产生粒子特效的事件
    /// </summary>
    public static event Action<ParticleEffectType, Vector3> ParticleEffectEvent;

    public static void CallParticelEffectEvent(ParticleEffectType effectType, Vector3 pos)
    {
        ParticleEffectEvent?.Invoke(effectType,pos);
    }

    /// <summary>
    /// 在场景一开始生成树石头稻草的事件
    /// </summary>
    public static event Action GenerateCropEvent;

    public static void CallGenerateCropEvent()
    {
        GenerateCropEvent?.Invoke();
    }

    //显示对话的事件
    public static event Action<DialoguePiece> ShowDialogueEvent;

    public static void CallShowDialogueEvent(DialoguePiece dialoguePiece)
    {
      
        ShowDialogueEvent?.Invoke(dialoguePiece);
    }

    //打开除玩家外背包的事件
    public static event Action<SlotType, InventoryBag_SO> BaseBagOpenEvent;

    public static void CallBaseBagOpenEvent(SlotType slotType, InventoryBag_SO inventoryBagSo)
    {
        BaseBagOpenEvent?.Invoke(slotType,inventoryBagSo);
    }
    //关闭除玩家外背包的事件
    public static event Action<SlotType, InventoryBag_SO> BaseBagCloseEvent;

    public static void CallBaseBagCloseEvent(SlotType slotType, InventoryBag_SO inventoryBagSo)
    {
        BaseBagCloseEvent?.Invoke(slotType,inventoryBagSo);
    }

    //更新游戏状态的事件
    public static event Action<GameState> UpdateGameStateEvent;

    public static void CallUpdateGameStateEvent(GameState gameState)
    {
        UpdateGameStateEvent?.Invoke(gameState);
    }
    
    //显示交易数量ui的事件
    public static event Action<ItemDetails,bool> ShowTradeUI;

    public static void CallShowTradeUI(ItemDetails itemDetails, bool isSell)
    {
        ShowTradeUI?.Invoke(itemDetails,isSell);
    }

    //建造家具的事件
    public static event Action<int,Vector3> BuildFurnitureEvent;

    public static void CallBuildFurnitureEvent(int ID,Vector3 mousePos)
    {
        BuildFurnitureEvent?.Invoke(ID,mousePos);
    }

    //切换灯光的事件
    public static event Action<Season, LightShift, float> LightShiftChangeEvent;

    public static void CallLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        LightShiftChangeEvent?.Invoke(season,lightShift,timeDifference);
    }

    //初始化声音的事件
    public static event Action<SoundDetails> InitSoundEffect;

    public static void CallInitSoundEffect(SoundDetails soundDetails)
    {
        InitSoundEffect?.Invoke(soundDetails);
    }

    //播放声音的方法
    public static event Action<SoundName> PlaySoundEvent;

    public static void CallPlaySoundEvent(SoundName soundName)
    {
        PlaySoundEvent?.Invoke(soundName);
    }

    //开始新游戏的事件
    //所有与保存相关的管理者订阅此事件 在开始新游戏的进行各自的工作
    public static event Action<int> StartNewGameEvent;

    public static void CallStartNewGameEvent(int index)
    {
        StartNewGameEvent?.Invoke(index);
    }

    //结束游戏的事件
    public static event Action EndGameEvent;

    public static void CallEndGameEvent()
    {
        EndGameEvent?.Invoke();
    }
    



}