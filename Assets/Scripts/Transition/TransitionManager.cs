using System;
using System.Collections;
using LYFarm.Camera;
using LYFarm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace LYFarm.Transition
{
    /// <summary>
    /// 负责场景切换相关的内容
    /// </summary>
    public class TransitionManager : Singleton<TransitionManager>,ISaveable
    {
        /// <summary>
        /// 游戏开始的时候需要加载的地图场景名称
        /// </summary>
        [SceneName]
        public string startSceneName = String.Empty;

       
        private CanvasGroup fadeCanvasGroup;

        private bool isFade;

        protected override void Awake()
        {
            base.Awake();
            //游戏一开始先加载ui
            SceneManager.LoadScene("UIScene", LoadSceneMode.Additive);
        }

        private void OnEnable()
        {
            //注册场景切换的事件
            EventHandler.TransitionEvent += OnTransitionEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            StartCoroutine(UnLoadScene());
        }

        private IEnumerator UnLoadScene()
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return Fade(1);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            yield return Fade(0);
        }
        private void OnStartNewGameEvent(int obj)
        {
            StartCoroutine(LoadSaveDataScene(startSceneName));
            
        }


        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            if (fadeCanvasGroup == null)
            { 
                fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
            }
           
        }

        // private IEnumerator Start()
        // {
        //     //注册
        //     ISaveable saveable = this;
        //     saveable.RegisterSaveable();
        //     
        //     //如果开始场景已没有加载 则不需要在加载场景
        //     if (!SceneManager.GetSceneByName(startSceneName).isLoaded)
        //     {
        //         //游戏开始加载第一个场景
        //         yield return StartCoroutine(LoadSceneSetActive(startSceneName));
        //     }
        //     //如果开始场景已经被加载则设置其为激活场景
        //     else
        //     {
        //         SceneManager.SetActiveScene(SceneManager.GetSceneByName(startSceneName));
        //     }
        //     //获取渐入渐出的CanvasGroup
        //     if (fadeCanvasGroup == null)
        //     {
        //         fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
        //     }
        //     EventHandler.CallAfterPlayerMoveEvent();
        //     /*
        //      * 游戏开始执行一次加载后的操作
        //      * 1、这里主要是为了获取游戏的Gird
        //      */
        //     EventHandler.CallAfterSceneLoadedEvent();
        // }
        /// <summary>
        /// 响应人物切换场景事件的方法
        /// </summary>
        /// <param name="sceneName">目标场景名称</param>
        /// <param name="positionToGo">在目标场景的位置</param>
        private void OnTransitionEvent(string sceneName, Vector3 positionToGo)
        {
            //淡入淡出结束才可以切换场景
            if(!isFade)
                StartCoroutine(Transition(sceneName, positionToGo));
        }

        /// <summary>
        /// 从一个地图切换到另一个地图
        /// </summary>
        /// <param name="sceneName">需要切换的地图名称</param>
        /// <param name="targetPosition">切换到场景后玩家的位置</param>
        /// <returns></returns>
        private IEnumerator Transition(string sceneName, Vector3 targetPosition)
        {
            
            
            /* 执行加载需要切换的地图前需要做的事情
             * 1、切换玩家不可以移动 切换为默认动画 并且隐藏举起的物品 
             * 2、取消背包格子的高亮
             * 3、保存当前场景中的物体
             */
            EventHandler.CallBeforeSceneUnloadEvent();
            
            //渐出
            yield return Fade(1);
            
            //设置相机跟随速度为最快
            //CameraManager.Instance.AfterFadeOut();
            
            //卸载当前的地图
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            
            //因为携程返回的就是携程所以不需要在使用startCoroutine方法去加载新的场景
            yield return LoadSceneSetActive(sceneName);
            
            /* 移动玩家的位置
             * 
             * 
             */
            EventHandler.CallMovePlayerToPosition(targetPosition);
            
            /* 主角移动完成后需要执行的方法
             * 1、切换摄像机碰撞区域
             * 2、重新获取当前地图的ItemParent（所有掉落物的父物体）
             * 3、在itemParent下生成保存的物体
             * 4、
             */
            EventHandler.CallAfterPlayerMoveEvent();
            
            //设置回来相机跟随速度为默认
            // CameraManager.Instance.AfterFadeIn();
            //渐入
            yield return Fade(0);
            /* 执行切换地图后需要做的事情
             * 1、设置主角可以移动
             * 2、
             * 3、
             */
            EventHandler.CallAfterSceneLoadedEvent();
            //OPTIMIZE: 在渐进开始的时候玩家就可以移动 修复
            
           
          
          
            
        }

        /// <summary>
        /// 加载场景并且设置为激活
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns></returns>
        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            //加载场景 并且是在原有的场景基础上加载
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            //获取新加载的场景
            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            //显示出来场景
            SceneManager.SetActiveScene(newScene);
        }

        /// <summary>
        /// 控制淡入淡出
        /// </summary>
        /// <param name="targetAlpha">目标Alpha值  0:淡入  1:淡出</param>
        /// <returns></returns>
        private IEnumerator Fade(float targetAlpha)
        {
            isFade = true;
            fadeCanvasGroup.blocksRaycasts = true;

            float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha)/Settings.fadeDuration;
            //数学的方式
            //Approximately两个值是否相似
            while (!Mathf.Approximately(fadeCanvasGroup.alpha,targetAlpha))
            {
                //更改Alpha值
                fadeCanvasGroup.alpha = 
                    Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }

            fadeCanvasGroup.blocksRaycasts = false;
            isFade = false;
        }


        private IEnumerator LoadSaveDataScene(string sceneName)
        {
            //渐出
            yield return Fade(1);
            //游戏过程加载新的游戏 
            if (SceneManager.GetActiveScene().name != "PersistentScene")
            {
                //执行卸载之前的事件
                EventHandler.CallBeforeSceneUnloadEvent();
                //卸载当前场景 
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
            }
            //代表从主菜单加载游戏
            
            yield return LoadSceneSetActive(sceneName);

            //呼叫加载场景之后的事件
            EventHandler.CallAfterPlayerMoveEvent();
            EventHandler.CallAfterSceneLoadedEvent();
            //渐入
            yield return Fade(0);

        }
        public string Guid => GetComponent<DataGUID>().guid;
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            //储存场景名称
            saveData.dataSceneName = SceneManager.GetActiveScene().name;
            Debug.Log("保存"+saveData.dataSceneName);
            return saveData;
        }

        public void RestoreData(GameSaveData gameSaveData)
        {
            //加载指定场景
            StartCoroutine(LoadSaveDataScene(gameSaveData.dataSceneName));
        }
        
        
    }
}
