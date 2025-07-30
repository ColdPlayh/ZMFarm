
using UnityEngine;

namespace LYFarm.Inventory
{
    public class ItemPickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            Item item = other.GetComponent<Item>();
            if (item != null)
            {
                if (item.itemDeails.canPickedUp)
                {
                    //拾取物品添aaaaa加到背包中
                    InventoryManager.Instance.AddItem(item,true);
                    //播放音效
                    EventHandler.CallPlaySoundEvent(SoundName.Pickup);
                }
            }
        }
    }
}

