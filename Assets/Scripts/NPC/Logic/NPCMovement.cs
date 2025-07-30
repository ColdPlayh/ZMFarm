
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using LYFarm.AStarN;
using LYFarm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LYFarm.NPC
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class NPCMovement : MonoBehaviour,ISaveable
    {
        #region 字段

        //npc日程数据
        public ScheduleDataList_SO scheduleData;
        //存放日程数据的排序集合
        private SortedSet<ScheduleDetails> scheduleSet;
        //当前日程
        private ScheduleDetails currentSchedule;

        //是否可以互动
        public bool isInteractable;
        [SerializeField]
        //当前所处场景
        [SceneName]
        public string currentScene;
        //目标场景
        private string targetScene;

        //当前的网格坐标
        private Vector3Int currentGridPosition;
        //目标网格坐标
        private Vector3Int targetGridPosition;
        //下一步网格的坐标
        private Vector3Int nextGridPosition;

        private Vector3 nextWorldPosition;
        //默认第一个场景
        public string StartScene
        {
            set => currentScene = value;
        }

        [Header("移动属性")] 
        public float normalSpeed = 2f;

        private float minSpeed = 1f;
        private float maxSpeed = 3f;
        
        //方向
        private Vector2 dir;
        //是否在移动
        [SerializeField]
        public bool isMoving;


        //组件
        private Rigidbody2D rigidbody2D;
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D coil;
        private Animator anim;
        private Grid grid;
        
        //存放路径的栈
        public Stack<MovementStep> movementSteps;

        //是否已经初始化过
        private bool isInitialized;
        
        //是否第一次加载
        private bool isFirstLoad;

        //npc是否在移动
        private bool npcMove;

        //场景是否加载完成
        private bool sceneLoaded;

        private Season currentSeason;
        
        //npc不移动时播放动画相关

        private float animationBreakTime;

        //是否可以播放动作
        private bool canPlayStopAnimation;

        //停止时候播放的动作
        private AnimationClip stopAnimationClip;

        //
        public AnimationClip blankAnimationClip;

        //anim
        private AnimatorOverrideController animOverride;


        //npc移动的携程
        private Coroutine npcMoveRoutine;
        //时间戳
        private TimeSpan GameTime => TimeManager.Instance.timeSpan;
        
        #endregion
        
        
        
        
        #region 生命周期方法
        
        private void Awake()
        {
            rigidbody2D = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            coil = GetComponent<BoxCollider2D>();
            anim = GetComponent<Animator>();
            movementSteps = new Stack<MovementStep>();
            
            //覆盖anim
            animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
            anim.runtimeAnimatorController = animOverride;
            
            scheduleSet = new SortedSet<ScheduleDetails>();
            foreach (var schedule in scheduleData.scheduleList)
            {
                scheduleSet.Add(schedule); 
            }
        }

        private void OnEnable()
        {
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.AfterPlayerMoveEvent += OnAfterPlayerMoveEvent;
            EventHandler.GameMinuteEvent += OnGameMinuteEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;

        }

        
        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        private void Update()
        {
            //场景加载完成后才可以切换动画
            if(sceneLoaded)
                SwitchAnimation();
          
            animationBreakTime -= Time.deltaTime;
            canPlayStopAnimation = animationBreakTime <= 0;
        }

        private void FixedUpdate()
        {
            if(sceneLoaded)
                Movement();
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
            EventHandler.AfterPlayerMoveEvent -= OnAfterPlayerMoveEvent;
        }

        

        private void OnStartNewGameEvent(int obj)
        {
            isInitialized = false;
            isFirstLoad = true;
        }

        #endregion
        
        #region 事件方法
        
        private void OnEndGameEvent()
        {
            //场景未加载
            sceneLoaded = false;
            //npc不能移动
            npcMove = false;
            if(npcMoveRoutine is not null)
                StopCoroutine(npcMoveRoutine);
        }

        private void OnBeforeSceneUnloadEvent()
        {
            sceneLoaded = false;
        }
        private void OnAfterPlayerMoveEvent()
        {
            CheckVisiable();   
        }
        /// <summary>
        /// 场景加载后 获取grid并且检查npc是否显示 并且初始化npc
        /// </summary>
        private void OnAfterSceneLoadedEvent()
        {
            sceneLoaded = true;
            grid = FindObjectOfType<Grid>();
            //OPTIMIZE check应该在fade前
            CheckVisiable();
            //如果没有初始化过
            if (!isInitialized)
            {
                //初始化npc
                InitNPC();
                isInitialized = true;
            }

            if (!isFirstLoad)
            {
                currentGridPosition = grid.WorldToCell(transform.position);
                ScheduleDetails schedule = new ScheduleDetails(0,0,0,0,
                    currentSeason,targetScene,(Vector2Int)targetGridPosition,stopAnimationClip,isInteractable);
                BuildPath(schedule);
                isFirstLoad = true;
            }
                
        }
        private void OnGameMinuteEvent(int minute, int hour,int day,Season gameSeason)
        {
            int time = (hour * 100) + minute;
            currentSeason = gameSeason;
            ScheduleDetails matchSchedule=null;
            //获取匹配的日程
            foreach (var schedule in scheduleSet)
            {
                if (schedule.ExecuteTime == time)
                {
                    //BUG:条件更改了
                    //if (schedule.day != day && schedule.day!=0)
                    // if (schedule.day!=0)
                    //     continue;
                    if(gameSeason!=schedule.season)
                        continue;
                    //获得匹配的日程
                    matchSchedule = schedule;
                }
                else if(schedule.ExecuteTime>time)
                {
                    break;
                }
            }

            //创建日程
            if (matchSchedule is not null)
            {
                BuildPath(matchSchedule);
            }
        }
        #endregion
        

        
        
        /// <summary>
        /// 初始化npc的方法
        /// </summary>
        private void InitNPC()
        {
            //设置目标场景
            targetScene = currentScene;
            //人物处于网格中心
            currentGridPosition = grid.WorldToCell(transform.position);
            transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2,
                currentGridPosition.y + Settings.gridCellSize / 2, 0);
            targetGridPosition = currentGridPosition;

        }

        private void Movement()
        {
            //如果npc在移动则需要等待移动完成
            if (npcMove) return;
           
            //有移动的步骤
            if (movementSteps.Count > 0)
            {
                //获取一个步骤
                MovementStep step = movementSteps.Pop();
                currentScene = step.sceneName;
                //检查npc是否显示
                CheckVisiable();
                //获取下一步的网格坐标
                nextGridPosition = (Vector3Int)step.gridCoordinate;
                //获得走到当前点位的时间戳
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);
                //移动位置
                MoveToGridPosition(nextGridPosition,stepTime);
            }
            
            else if (!isMoving && canPlayStopAnimation)
            {
                anim.SetFloat("DirX",0);
                anim.SetFloat("DirY",-1);
                if(stopAnimationClip is not null)
                    StartCoroutine(SetStopAnimation());
            }
        }

        private IEnumerator SetStopAnimation()
        {
           
            //重置计时器
            animationBreakTime = Settings.animationBreakTime;

            if (stopAnimationClip is not null)
            {
                animOverride[blankAnimationClip] = stopAnimationClip;
                anim.SetBool("EventAnimation",true);
                yield return null;
                anim.SetBool("EventAnimation",false);
            }
            else
            {
                animOverride[stopAnimationClip] = blankAnimationClip;
                anim.SetBool("EventAnimation",false);
            }
            
        }
        private void MoveToGridPosition(Vector3Int gridPos, TimeSpan stepTime)
        {
            npcMoveRoutine=StartCoroutine(MoveRoutine(gridPos, stepTime));
        }

        private IEnumerator MoveRoutine(Vector3Int gridPos, TimeSpan stepTime)
        {
            //设置正在移动
            npcMove = true;
            //获得世界坐标
            nextWorldPosition = GetWorldPos(gridPos);
            //还有时间来进行移动
            if (stepTime > GameTime)
            {
                //得到移动到下一个网格所剩余的秒数
                float TimeToMove = (float) (stepTime.TotalSeconds - GameTime.TotalSeconds);

                //获取和目标点的距离
                float distance = Vector3.Distance(transform.position, nextWorldPosition);
                //计算移动速度
                float speed = Mathf.Max(minSpeed, distance / TimeToMove/Settings.secondThreshold);
                //移动
                if (speed <= maxSpeed)
                {
                    while (Vector3.Distance(transform.position,nextWorldPosition)>0.01f)
                    {
                        dir = (nextWorldPosition - transform.position).normalized;
                        Vector2 posOffset = new Vector2
                            (dir.x * speed * Time.fixedDeltaTime,dir.y*speed*Time.fixedDeltaTime);
                        
                        rigidbody2D.MovePosition(rigidbody2D.position+posOffset);

                        yield return new WaitForFixedUpdate();
                    }
                }
                
            }
            //如果时间来不及就瞬移过去
            rigidbody2D.position = nextWorldPosition;
            currentGridPosition = gridPos;
            nextGridPosition = currentGridPosition;

            npcMove = false;
        }
        /// <summary>
        /// 构建路径的方法
        /// </summary>
        /// <param name="schedule"></param>
        public void BuildPath(ScheduleDetails schedule)
        {
            //清空路径
            movementSteps.Clear();
            //获取日程
            currentSchedule = schedule;
            targetScene = schedule.targetScene;
            targetGridPosition = (Vector3Int)schedule.targetGridPosition;
            stopAnimationClip = schedule.clipAtStop;
            //设置是否可以互动
            this.isInteractable = schedule.isInteractable;
            //同场景
            if (schedule.targetScene == currentScene)
            {
                Debug.Log("同场景");
               AStar.Instance.BuildPath(schedule.targetScene,(Vector2Int)currentGridPosition,
                   schedule.targetGridPosition,movementSteps);
               
               
            }
            //跨场景
            else if (schedule.targetScene != currentScene)
            {
                 Debug.Log("跨场景");
                SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);
                if (sceneRoute is not null)
                {
                    for (int i = 0; i < sceneRoute.scenePathList.Count; i++)
                    {
                        Vector2Int fromPos, gotoPos;
                        //在当前场景只有targetPath 而fromPath是99999 这个时候frompath应该是当前的坐标
                        ScenePath path = sceneRoute.scenePathList[i];
                        if (path.fromGridCeil.x >= Settings.maxGridSize ||
                            path.fromGridCeil.y >= Settings.maxGridSize)
                        {
                            fromPos = (Vector2Int)currentGridPosition;
                            
                        }
                        //如果frompath存在的话代表现在是我们需要跨越的场景 
                        else
                        {
                            fromPos = path.fromGridCeil;
                        }
                        // Debug.Log("跨场景1"+sceneRoute.fromSceneName+":"+sceneRoute.gotoSceneName+
                                  // fromPos+currentGridPosition);
                        // Debug.Log("跨场景 goto"+path.gotoGridCeil.x+"from"+path.fromGridCeil.x);
                        //代表着是跨越之后的场景 目标位置就是我们的目标位置 
                        if (path.gotoGridCeil.x >= Settings.maxGridSize ||
                            path.gotoGridCeil.y >= Settings.maxGridSize)
                        {
                           
                            gotoPos = schedule.targetGridPosition;
                            // Debug.Log("schedule跨场景");
                        }
                        
                        else
                        {
                            gotoPos = path.gotoGridCeil;
                            // Debug.Log("gotogridcel跨场景");
                        }
                        // Debug.Log("跨场景2"+sceneRoute.fromSceneName+":"+sceneRoute.gotoSceneName+
                        //           gotoPos+schedule.targetGridPosition);
                        Debug.Log("跨场景"+sceneRoute.fromSceneName+":"+sceneRoute.gotoSceneName+" 路线"+
                                  fromPos+gotoPos);
                        AStar.Instance.BuildPath(path.sceneName,fromPos,gotoPos,movementSteps);
                        
                    }
                }
                
            }
            
            //如果成功构建路径
            if (movementSteps.Count > 1)
            {
                //更新时间戳
                UpdateTimeOnPath();
            }
        }

        /// <summary>
        /// 更新每一步的时间戳
        /// </summary>
        public void UpdateTimeOnPath()
        {
            MovementStep previousStep=null;
            TimeSpan currentGameTime = GameTime;
            foreach (MovementStep step in movementSteps)
            {
                //如果是第一步 则初始化第一步
                if (previousStep is null)
                    previousStep = step;
                //设置每一步的时间
                step.hour = currentGameTime.Hours;
                step.minute = currentGameTime.Minutes;
                step.second = currentGameTime.Seconds;
                TimeSpan gridMovementStepTime;
                //如果是斜方向 
                if (MoveInDiagonal(step, previousStep))
                {
                    gridMovementStepTime = new TimeSpan(0, 0,
                        (int) (Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
                }
                //不是斜方向
                else
                {
                    gridMovementStepTime = new TimeSpan(0, 0,
                        (int) (Settings.gridCellSize / normalSpeed / Settings.secondThreshold));
                }
                //获得当前步骤的时间戳
                currentGameTime = currentGameTime.Add(gridMovementStepTime);
                //记录上一步
                previousStep = step;
            }

            foreach (var step in movementSteps)
            {
                Debug.Log("时间"+step.hour+":"+step.minute+":"+step.second);
            }
        }
        
        /// <summary>
        /// 判断当前要走的这一步是不是上一步斜着走过来的
        /// </summary>
        /// <param name="currentStep"></param>
        /// <param name="previousStep"></param>
        /// <returns></returns>
        private bool MoveInDiagonal(MovementStep currentStep, MovementStep previousStep)
        {
            
            //如果xy都不相同代表是斜方向 
            return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) &&
                   (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y);
        }

        private Vector3 GetWorldPos(Vector3Int gridPos)
        {
            Vector3 worldPos = grid.CellToWorld(gridPos);
            return new Vector3(worldPos.x + Settings.gridCellSize / 2, worldPos.y + Settings.gridCellSize / 2, 0);
        }
        #region 设置npc显示
        private void CheckVisiable()
        {
            if (currentScene == SceneManager.GetActiveScene().name) 
            {
                SetActiveInScene();
            }
            else
            {
                SetInactiveInScene();
            }
        }
        private void SetActiveInScene()
        {
            spriteRenderer.enabled = true;
            coil.enabled = true;
            //TODO:显示影子
             transform.GetChild(0).gameObject.SetActive(true);
            
        }
        private void SetInactiveInScene()
        {
            spriteRenderer.enabled = false;
            coil.enabled = false;
            //TODO:隐藏影子
            transform.GetChild(0).gameObject.SetActive(false);
        }

        
        #endregion

        private void SwitchAnimation()
        {
            isMoving = transform.position != GetWorldPos(targetGridPosition);
            //切换动画
            anim.SetBool("IsMoving",isMoving);
            if (isMoving)
            {
                //立刻退出其他动作
                anim.SetBool("Exit",true);
                //设置速度
                anim.SetFloat("DirX",dir.x);
                anim.SetFloat("DirY",dir.y);
            }
            else
            {
                anim.SetBool("Exit",false);
            }
        }

        public string Guid => GetComponent<DataGUID>().guid;
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
            //保存目标网格位置
            saveData.characterPosDict.Add("targetGridPosition",new SerializableVector3(targetGridPosition));
            //当前所处于的网格坐标
            saveData.characterPosDict.Add("currentPosition",new SerializableVector3(transform.position));
            //保存当前场景
            saveData.dataSceneName = currentScene;
            //保存目标场景
            saveData.targetScene = this.targetScene;
            //保存停止移动时候的动画
            if (stopAnimationClip is not null)
            {
                saveData.animationInstanceID = stopAnimationClip.GetInstanceID();
            }
            //保存是否可以互动
            saveData.interactable = isInteractable;
            saveData.timeDict = new Dictionary<string, int>();
            saveData.timeDict.Add("currentSeason",(int)currentSeason);
            return saveData;

        }

        public void RestoreData(GameSaveData gameSaveData)
        {
            //加载存档的时候人物一定是初始化过的
            isInitialized = true;
            //不是第一次加载
            isFirstLoad = false;
            //读取当前场景和目标场景
            currentScene = gameSaveData.dataSceneName;
            targetScene = gameSaveData.targetScene;
            //读取当前位置和目标位置
            Vector3 pos = gameSaveData.characterPosDict["currentPosition"].ToVector3();
            Vector3Int targetGridPos= (Vector3Int)gameSaveData.characterPosDict["targetGridPosition"].ToVector2Int();
            transform.position = pos;
            targetGridPosition = targetGridPos;

            //读取动画
            if (gameSaveData.animationInstanceID != 0)
            {
                this.stopAnimationClip =Resources.InstanceIDToObject(gameSaveData.animationInstanceID) as AnimationClip;
            }
            //读取是否可以互动
            this.isInteractable = gameSaveData.interactable;
            //获取季节
            this.currentSeason =(Season) gameSaveData.timeDict["currentSeason"];

        }
    }
}

