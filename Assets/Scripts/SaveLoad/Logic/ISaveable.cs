using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LYFarm.Save
{
    public interface ISaveable
    {
        string Guid { get; }

        //注册的方法
        void RegisterSaveable()
        {
            SaveLoadManager.Instance.RegisterSaveable(this);
        }
        //返回保存的数据
        GameSaveData GenerateSaveData();

        //加载数据
        void RestoreData(GameSaveData gameSaveData);
        
    }
}
