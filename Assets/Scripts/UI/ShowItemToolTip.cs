using UnityEngine;
using UnityEngine.EventSystems;


namespace LYFarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ShowItemToolTip : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        private SlotUI slotUI;
        private InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();
        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
           
            if (slotUI.itemDetails!=null)
            {
                //显示物品信息
                inventoryUI.ItemToolTip.gameObject.SetActive(true);
                inventoryUI.ItemToolTip.SetupToolTip(slotUI.itemDetails,slotUI.slotType);
                inventoryUI.ItemToolTip.GetComponent<RectTransform>().pivot= new Vector2(0.5f, 0);
                inventoryUI.ItemToolTip.transform.position = transform.position + Vector3.up * 60;
              
                //如果是图纸
                if (slotUI.itemDetails.itemType == ItemType.Furniture)
                {
                    inventoryUI.ItemToolTip.resoucePanel.SetActive(true);
                    inventoryUI.ItemToolTip.SetupResoucePanel(slotUI.itemDetails.itemID);
                }
                else
                {
                    inventoryUI.ItemToolTip.resoucePanel.SetActive(false);
                }

            }
            else
            {
                inventoryUI.ItemToolTip.gameObject.SetActive(false);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inventoryUI.ItemToolTip.gameObject.SetActive(false);
        }
    }
}

