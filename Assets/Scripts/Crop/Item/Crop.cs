using System.Collections;
using System.Collections.Generic;
using Audio.Data;
using UnityEngine;

public class Crop : MonoBehaviour
{
    public CropDetails cropDetails;
    [HideInInspector]
    public TileDetails tileDetails;

    public bool canHarvest => tileDetails.growthDays >= cropDetails.TotalGrowthDays;
    
    private int harvestActionCount;
    private Animator anim;
    private Transform playerTransform => FindObjectOfType<Player>().transform;
    /// <summary>
    /// 在背包或者世界中生成农作物
    /// </summary>
    /// <param name="tool"></param>
    public void ProcessToolAction(ItemDetails tool,TileDetails tile)
    {
       
        int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);

        if (requireActionCount == -1)
            return;
        tileDetails = tile;
        anim = GetComponentInChildren<Animator>();

        if (harvestActionCount < requireActionCount)
        {
            
            
            harvestActionCount++;
       
            //TODO:判断作物本身是否存在动画
            //TODO:播放动画 粒子效果 播放声音
            if (anim is not null && cropDetails.hasAnimation)
            {
                //玩家在左 往右倒
                if (playerTransform.position.x < transform.position.x)
                {
                    anim.SetTrigger("RotateRight");
                }
                else
                {
                    anim.SetTrigger("RotateLeft");
                }
               
            }
            if (cropDetails.hasParticalEffect)
            {
                //播放粒子效果
                EventHandler.CallParticelEffectEvent
                    (cropDetails.particleEffectType,transform.position+cropDetails.effectPos);
            }
            //TODO:播放声音
            if (cropDetails.soundEffect != SoundName.none)
            {
               EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
            }
            
        }
    
        if (harvestActionCount >= requireActionCount)
        {
       
            //如果生成在player头顶
            if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
            {
                
                //生成农作物\
                SpawnHarvestItem();
            }
            //如果是生成掉落物
            else if (cropDetails.hasAnimation)
            {
                //树木倒下
                if (playerTransform.position.x < transform.position.x)
                {
                    anim.SetTrigger("FallingRight");
                }
                else
                {
                    anim.SetTrigger("FallingLeft");
                }
                //播放树倒下的声音
                EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                StartCoroutine(HarvestAfterAnimation());
            }
            
            
        }
        
    }

    private IEnumerator HarvestAfterAnimation()
    {
        //如果砍到动画没有结束 等待其结束
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("End"))
        {
            yield return null;
        }
        //生成收获的物品
        SpawnHarvestItem();
        //转换新物体
        if (cropDetails.transferItemID > -1)
        {
            CreateTransferCrop();
        }
        
    }

    private void CreateTransferCrop()
    {
        tileDetails.seedItemID = cropDetails.transferItemID;
        tileDetails.daySinceLastHarvest = -1;
        tileDetails.growthDays = 0;
        EventHandler.CallRefreshCurrentMap();
    }
    /// <summary>
    /// 生成收获的物品
    /// </summary>
    public void SpawnHarvestItem()
    {
        for (int i = 0; i < cropDetails.producedItemID.Length; i++)
        {
            int amountToProduce;
            //如果最大掉落数量和最小掉落数量相同
            if (cropDetails.producedMaxAmount[i] == cropDetails.produceMinAmount[i])
            {
                amountToProduce = cropDetails.produceMinAmount[i];
            }
            //如果不同
            else
            {
                amountToProduce = Random.Range(cropDetails.produceMinAmount[i], cropDetails.producedMaxAmount[i]);

            }
            //生成每一个物体
            for (int j = 0; j < amountToProduce; j++)
            {
                //如果是直接生成到player背包里面
                if (cropDetails.generateAtPlayerPosition)
                {
                    EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
                }
                //生成在世界中
                else
                {
                    var dirX = transform.position.x > playerTransform.position.x ? 1 : -1;
                    //随机生成位置
                    var spawnPos = new Vector3(
                        transform.position.x + Random.Range(dirX, cropDetails.produceRadius.x * dirX),
                        transform.position.y + Random.Range(-cropDetails.produceRadius.y, cropDetails.produceRadius.y),
                        0);
                    //
                    EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i],spawnPos);
                }
            }
        }
        //TODO:重复生长的逻辑
        if (tileDetails is not null)
        {
            tileDetails.daySinceLastHarvest++;
            if (cropDetails.daysInRegrow > 0 && tileDetails.daySinceLastHarvest<cropDetails.regrowTimes)
            {
                tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysInRegrow;
                //刷新种子
                EventHandler.CallRefreshCurrentMap();
            }
            else
            {
                tileDetails.daySinceLastHarvest = -1;
                tileDetails.seedItemID = -1;
                
                
            }
            Destroy(gameObject);
        }
    }
}
