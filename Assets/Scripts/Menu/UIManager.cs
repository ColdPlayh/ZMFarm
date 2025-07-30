using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WaitForSeconds = UnityEngine.WaitForSeconds;

public class UIManager : Singleton<UIManager>
{
    [HideInInspector]
    public GameObject menuCanvas;
    public GameObject menuPrefab;

    public Button settingsBtn;
    public GameObject pausePanel;
    public Slider musicVolumeSlider;

   
    

    private void Awake()
    {
        
        settingsBtn.onClick.AddListener(TogglePausePanel);
        //设置音量
        musicVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }

    private void Start()
    {
        menuCanvas = GameObject.FindGameObjectWithTag("MenuCanvas");
        Instantiate(menuPrefab, menuCanvas.transform);
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    private void OnAfterSceneLoadedEvent()
    {
        //加载场景后销毁menuCanvas
        if (menuCanvas.transform.childCount > 0)
        {
            Destroy(menuCanvas.transform.GetChild(0).gameObject);
        }
    }

    //控制启动暂停菜单
    private void TogglePausePanel()
    {
        bool isOpen = pausePanel.activeInHierarchy;
        if (isOpen)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            System.GC.Collect(1);
            pausePanel.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void ReturnMenuCanvas()
    {
        Time.timeScale = 1;
        StartCoroutine(BackToMenu());
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator BackToMenu()
    {
        //关闭暂停菜单
        pausePanel.SetActive(false);
        //呼叫结束游戏的事件
        EventHandler.CallEndGameEvent();
        //生成menu
        yield return new WaitForSeconds(0.75f);
        Instantiate(menuPrefab, menuCanvas.transform);
       
    }

    
}
