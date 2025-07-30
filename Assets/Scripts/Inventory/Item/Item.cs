using System;
using LYFarm.CropPlant;
using UnityEngine;

namespace LYFarm.Inventory
{
    /// <summary>
    /// 世界中一个掉落物品的模板
    /// </summary>
    public class Item : MonoBehaviour
    {
        public int itemID;
        private BoxCollider2D boxCollider;
        private SpriteRenderer spriteRenderer;
       
        public ItemDetails itemDeails;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if (itemID != 0)
            {
                Init(itemID);
            }
        }

        public void Init(int ID)
        {
            itemID = ID;
            
            //InventoryManager拿数据
            itemDeails = InventoryManager.Instance.GetItemDetails(itemID);
            if (itemDeails != null)
            {
                //设置Sprite
                spriteRenderer.sprite = 
                    itemDeails.itemOnWorldSprite !=null ? itemDeails.itemOnWorldSprite : itemDeails.itemIcon;
                //设置BoxCollider的大小
                Vector2 newSize = new Vector2(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
                boxCollider.size = newSize;
                //spriteRenderer.sprite.bounds.center=当前图片中心点的坐标
                //所以直接把offset的y值设置为center的y即可
                boxCollider.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);
            }

            //如果是杂草 就初始化杂草
            if (itemDeails.itemType == ItemType.ReapableScenery)
            {
                gameObject.AddComponent<ReapItem>().InitCropData(itemDeails.itemID);
                gameObject.AddComponent<ItemInterActive>();
            }
        }
    }
}
