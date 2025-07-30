using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;

namespace LYFarm.Save
{
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        //保存所有需要储存的对象
        private List<ISaveable> saveableList = new List<ISaveable>();

        /// <summary>
        /// 保存每个进度的列表
        /// </summary>
        public List<DataSlot> saveSlotList = new List<DataSlot>(new DataSlot[3]);

        //保存文件路径
        private string jsonFolder;

        //当前点击进度的序号
        private int currentDataIndex;

        public bool isNewGame;
        
        protected override void Awake()
        {
            base.Awake();
            jsonFolder = Application.persistentDataPath + "/Save Data/";
            //游戏一开始得到所有的进度
            ReadSaveData();
        }

        private void OnEnable()
        {
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void Update()
        {
            //测试
            if (Input.GetKeyDown(KeyCode.I))
            {
                Save(currentDataIndex);
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                Load(currentDataIndex);
            }
        }

        private void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            Save(currentDataIndex);
        }

        private void OnStartNewGameEvent(int index)
        {
            //设置当前点击的进度的index
            currentDataIndex = index;
        }

        //游戏开始所有需要保存的对象都会注册到列表中
        public void RegisterSaveable(ISaveable saveable)
        {
            if (!saveableList.Contains(saveable))
            {
                saveableList.Add(saveable);
            }
        }

        public void ReadSaveData()
        {
            if (Directory.Exists(jsonFolder))
            {
                for (int i = 0; i < saveSlotList.Count; i++)
                {
                    var resultPath = jsonFolder + "save_" + i + ".sav";
                    if (File.Exists(resultPath))
                    {
                        Debug.Log("读取"+resultPath);
                        //读取文件
                        string stringData = File.ReadAllText(resultPath);
                        //转化为dataslot
                        DataSlot jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
                        //设置三个进度
                        saveSlotList[i] = jsonData;
                    }
                    
                }
            }
        }
        //保存数据
        private void Save(int index)
        {
            //创建每一个进度的数据
            DataSlot saveSlotData = new DataSlot();
            foreach (var saveable in saveableList)
            {
                saveSlotData.saveDataDict.Add(saveable.Guid,saveable.GenerateSaveData());
            }
            //保存到进度的列表中
            saveSlotList[index] = saveSlotData;
            //保存文件
            var resultPath = jsonFolder + "save_" + index+".sav";
            //序列化一个进度 Formatting.Indented 一行一行的序列化
            var jsonData = JsonConvert.SerializeObject(saveSlotList[index],Formatting.Indented);
            //当前路径不存在
            if (!File.Exists(resultPath))
            {
                //创建路径
                Directory.CreateDirectory(jsonFolder);
            }
            Debug.Log("Data:"+index+":"+resultPath);
            //创建文件
            File.WriteAllText(resultPath,jsonData);
            
        }

        //加载存档
        public void Load(int index)
        {
            //储存index
            currentDataIndex = index;
            string resultPath=jsonFolder + "save_" + index+".sav";
            //读取data
            string stringData = File.ReadAllText(resultPath);
            //得到DataSlot
            DataSlot jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
            foreach (var saveable in saveableList)
            {
                //让每一个脚本取执行读取的方法
                saveable.RestoreData(jsonData.saveDataDict[saveable.Guid]);
            }
        }
        
    }
}