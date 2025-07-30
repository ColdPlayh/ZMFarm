using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LYFarm.Save;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUi : MonoBehaviour
{
    //text
    public Text dataTime, dataScene;
    //button
    private Button currentButton;
    //当前按钮对应的进度
    private DataSlot currentDataSlot;

    private string jsonFolder;

    private Button deleteBtn;
    //获取在hierachey的序号
    private int Index => transform.GetSiblingIndex();
    private void Awake()
    {
        currentButton = GetComponent<Button>();
        currentButton.onClick.AddListener(LoadGameData);
        jsonFolder=Application.persistentDataPath + "/Save Data/";
    }

    private void OnEnable()
    {
        //返回主菜单也需要重新设置一下 （如果玩家在游戏中进行了保存）
        SetupSlotUI();
    }

    public void DeleteSaveSlot(int index)
    {
        var tarPath = jsonFolder + "save_" + index + ".sav";
        if(Directory.Exists(tarPath))
            File.Delete(tarPath);
        SaveLoadManager.Instance.ReadSaveData();
        SetupSlotUI();
    }
    private void SetupSlotUI()
    {
        //获得对应的进度
        currentDataSlot = SaveLoadManager.Instance.saveSlotList[Index];
        if (currentDataSlot is not null)
        {
            //设置时间文本
            dataTime.text = currentDataSlot.DataTime;
            //设置场景文本
            dataScene.text = currentDataSlot.GameScene;
            // deleteBtn.gameObject.SetActive(true);
        }
        else
        {
            dataTime.text = "世界充满着无尽的虚无";
            dataScene.text = "梦还未开始...";
            // deleteBtn.gameObject.SetActive(false);
        }
    }
    //点击当前button调用的方法
    private void LoadGameData()
    {
        //存在进度加载进度
        if (currentDataSlot != null)
        {
            
            SaveLoadManager.Instance.isNewGame = false;
            SaveLoadManager.Instance.Load(Index);
            
        }
        //不存在进度 开始一个新世界
        else
        {
            Debug.Log("新游戏"+Index);
            AudioManager.Instance.musicPause = true;
            SaveLoadManager.Instance.isNewGame = true;
            TimeLineManager.Instance.isPlayed = false;
            EventHandler.CallStartNewGameEvent(Index);
           
        }
    }
}
