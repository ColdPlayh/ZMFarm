

/// <summary>
/// <para>存储Item类型</para>
/// <param name="Seed">种子</param>
/// <param name="Commodity">商品</param>
/// <param name="Furniture">家具</param>
/// <param name="HoeTool">耕地工具</param>
/// <param name="ChopTool">砍伐工具</param>
/// <param name="BreakTool">采矿工具</param>
/// <param name="ReapTool">采集工具</param>
/// <param name="WaterTool">浇水工具</param>
/// <param name="CollectTool">收割工具</param>
/// <param name="ReapableScenery">杂草</param>
/// </summary>
public enum ItemType
{
   
    //种子，商品，家具
    Seed,Commodity,Furniture,
    //锄头，砍树工具，砸石头工具，割草工具,浇水工具，收割工具
    HoeTool,ChopTool,BreakTool,ReapTool,WaterTool,CollectTool,
    //杂草
    ReapableScenery
}

/// <summary>
/// <para>容器类型</para>
/// <param name="Bag">背包</param>
/// <param name="Box">盒子</param>
/// <param name="SHop">商店</param>
/// </summary>
public enum SlotType
{
    //背包，盒子，商店
    Bag,Box,Shop
}
/// <summary>
/// 当前slot所处的位置在哪里
/// </summary>
public enum InventoryLocation
{
    Player,Box
}

/// <summary>
/// 主角所拥有的部位的名称
///<para>注意：要和hierarchy窗口中设置的名称一致</para>
/// </summary>
public enum PartName
{
    Body,Hair,Arm,Tool
}

/// <summary>
/// 主角当前部位所处的动画状态
/// </summary>
public enum PartType
{
    None,Carry,Hoe,Water,Collect,Chop,Break,Reap,
}

public enum Season
{
    春天,夏天,秋天,冬天
}

//网格类型
public enum GirdType
{
    Diggable,DropItem,PlaceFurniture,NPCObstacle
}

//粒子效果类型
public enum ParticleEffectType
{
    None,LeavesFalling01,LeavesFalling02,Rock,ReapableScenery
}

//游戏状态
public enum GameState
{
    GamePlay,Pause
}

//光照状态
public enum LightShift
{
    Morning,Noon,Night
}

public enum SoundName
{
    none, FootStepSoft, FootStepHard,
    Axe, Pickaxe, Hoe, Reap, Water, Basket,
    Pickup, Plant, TreeFalling, Rustle,
    AmbientCountryside1, AmbientCountryside2, MusicCalm1, MusicCalm2, MusicCalm3, MusicCalm4, MusicCalm5, MusicCalm6, AmbientIndoor1
}


