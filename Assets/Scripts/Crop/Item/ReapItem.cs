using UnityEngine;



namespace LYFarm.CropPlant
{
    public class ReapItem : MonoBehaviour
    {
        private CropDetails cropDetails;
        private Transform playerTransform => FindObjectOfType<Player>().transform;

        public void InitCropData(int ID)
        {
            cropDetails=CropManager.Instance.GetCropDetails(ID);
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
                            transform.position.y +
                            Random.Range(-cropDetails.produceRadius.y, cropDetails.produceRadius.y),
                            0);
                        //
                        EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                    }
                }
            }
        }
    }
        
}

