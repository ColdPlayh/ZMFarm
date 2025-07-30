using System;
using LYFarm.Save;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

public class TimeLineManager : Singleton<TimeLineManager>
{
        //timeline
        public PlayableDirector startDirector;
        //当前的timeline
        private PlayableDirector currentDirector;
        //对话是否播放完毕
        private bool isDone;
        //timeline是否在暂停
        private bool isPuase;
        public bool isPlayed;
        public bool isPlaying;
        
    
        
        
        public bool IsDone
        {
                set => isDone = value;
        }
        protected override void Awake()
        {
                base.Awake();
                currentDirector = startDirector;
        }

        private void OnEnable()
        {
                EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        }
        private void Update()
        {
                //对话播放完毕按下空格可以继续播放
                if (isPuase && Input.GetKeyDown(KeyCode.Space) && isDone)
                {
                        isPuase = false;
                        currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
                }
        }
        private void OnDisable()
        {
                EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        }
        
        private void OnAfterSceneLoadedEvent()
        {
                if (isPlayed || !SaveLoadManager.Instance.isNewGame) return;
                //BUG:切换场景的时候都会播放动画
                currentDirector = FindObjectOfType<PlayableDirector>();
                if (currentDirector is not null)
                {
                        currentDirector.Play();
                }
        }
        
        //暂停timeline的方法
        public void PauseTimelIne(PlayableDirector director)
        {
                currentDirector = director;
                currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
                isPuase = true;
        }
       
}
