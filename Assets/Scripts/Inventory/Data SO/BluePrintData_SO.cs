using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Data_SO
{
    [CreateAssetMenu(fileName = "BluePrintData_SO",menuName = "Inventory/BluePrintData_SO")]
    public class BluePrintData_SO : ScriptableObject
    {
        
        public List<BluePrintDetails> bluePrintDetailsList;

        public BluePrintDetails GetBluePrintDetails(int itemID)
        {
            return bluePrintDetailsList.Find(b => b.ID == itemID);
        }
    }

    [Serializable]
    public class BluePrintDetails
    {
        public int ID;
        public InventoryItem[] resourceItems = new InventoryItem[4];
        public GameObject buildPrefab;
    }
}