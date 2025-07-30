using System;
using UnityEngine;


namespace LYFarm.Save
{
    [ExecuteAlways]
    public class DataGUID : MonoBehaviour
    {
        //guid 唯一性 来作为保存数据的键
        public string guid;

        private void Awake()
        {
            if (guid == string.Empty)
            {
                guid=System.Guid.NewGuid().ToString();
            }
        }
    }
}