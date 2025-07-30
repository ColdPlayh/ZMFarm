using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NPC.Data;
using UnityEngine;


namespace LYFarm.NPC
{
    public class NPCManager : Singleton<NPCManager>
    {
        public SceneRouteDataList_SO sceneRouteDataListSo;
        public List<NPCPosition> npcPositionList;

       
        private Dictionary<string, SceneRoute> sceneRouteDict = new Dictionary<string, SceneRoute>();


        protected override void Awake()
        {
            base.Awake();
            InitSceneRouteDict();
        }

        private void OnEnable()
        {
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;

        }

        private void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }

        private void OnStartNewGameEvent(int index)
        {
            foreach (var charater in npcPositionList)
            {
                //设置npc开始的位置和场景
                charater.npc.transform.position = charater.position;
                charater.npc.GetComponent<NPCMovement>().currentScene = charater.startScene;
            }
        }

        //初始化路线字典
        private void InitSceneRouteDict()
        {
            if (sceneRouteDataListSo.sceneRouteList.Count > 0)
            {
                foreach (SceneRoute route in sceneRouteDataListSo.sceneRouteList)
                {
                    var key = route.fromSceneName + route.gotoSceneName;
                    if(sceneRouteDict.ContainsKey(key))
                        continue;
                    else
                    {
                        sceneRouteDict.Add(key,route);
                    }
                    Debug.Log(key+":"+sceneRouteDict[key].scenePathList[0].fromGridCeil+sceneRouteDict[key].scenePathList[0].gotoGridCeil
                              +"跨场景"+sceneRouteDict[key].scenePathList[1].fromGridCeil+sceneRouteDict[key].scenePathList[1].gotoGridCeil);
                }
            }
            
        }

        /// <summary>
        /// 获取一个跨场景路径
        /// </summary>
        /// <param name="fromSceneName">来的场景</param>
        /// <param name="gotoSceneName">去的场景</param>
        /// <returns></returns>
        public SceneRoute GetSceneRoute(string fromSceneName, string gotoSceneName)
        {
            return sceneRouteDict[fromSceneName + gotoSceneName];
        }
    }

}
