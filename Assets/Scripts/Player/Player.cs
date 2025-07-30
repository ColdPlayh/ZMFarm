using System;
using System.Collections;
using System.Collections.Generic;
using LYFarm.Save;
using UnityEngine;

public class Player : MonoBehaviour,ISaveable
{
    private Rigidbody2D rb;

    [Header("速度")]
    //移动速度
    public float speed;
     
    //x轴的输入
    private float inputX; 
    //y轴的输入
    private float inputY;
    //根据x y轴的输入得到的二维向量
    private Vector2 movementInput;
    //Arm Body Hair的Animator
    private Animator[] animators;
    //判断是否在移动
    private bool isMoving;
    /// <summary>
    /// 键盘输入是否有效
    /// </summary>
    private bool inputDisable;

    private float mouseX;
    private float mouseY;
    private bool useTool;

    #region 生命周期函数
    
    private void Awake()
    {
        //获取组件
        rb = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
        inputDisable = true;
    }

    private void Start()
    {
        //注册到SaveLoadManager
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void OnEnable()
    {
        //将事件注册到注册场景卸载前的委托
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnLoadEvent;
        //将事件注册到场景加载后的委托
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvnet;
        //注册切换玩家位置的事件
        EventHandler.MovePlayerToPosition += OnMovePlayerToPosition;
        EventHandler.MouseClickEvent += OnMouseClickEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    //逻辑相关
    private void Update()
    {
        if(!inputDisable)
            PlayerInput();
        else
        {
            isMoving = false;
        }
        SwitchAnimation();
    }

    //物理相关
    private void FixedUpdate()
    {
        //移动
        if(!inputDisable)
            Movement();
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnLoadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvnet;
        EventHandler.MovePlayerToPosition -= OnMovePlayerToPosition;
        EventHandler.MouseClickEvent -= OnMouseClickEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    
    #endregion


    #region 事件方法

    private void OnEndGameEvent()
    {
        //关闭输入
        inputDisable = true;
    }

    private void OnStartNewGameEvent(int index)
    {
        //可以输入
        inputDisable = false;
        //设置位置
        transform.position = Settings.playerStartPos;
    }
    /// <summary>
    /// 场景卸载之前要调用的事件
    /// </summary>
    private void OnBeforeSceneUnLoadEvent()
    {
        //切换场景前玩家不可以输入
        inputDisable = true;
    }

    /// <summary>
    /// 场景加载完成后要调用的事件
    /// </summary>
    private void OnAfterSceneLoadedEvnet()
    {
        //切换场景后玩家可以输入
        inputDisable = false;
    }
    /// <summary>
    /// <para>需要移动玩家位置时要调用的事件</para>
    /// 切换玩家位置到目标位置
    /// </summary>
    /// <param name="targetPosition"></param>
    private void OnMovePlayerToPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }
    /// <summary>
    /// 鼠标点击后要调用的事件
    /// <para></para>
    /// </summary>
    /// <param name="mouseWorldPos">鼠标世界坐标</param>
    /// <param name="itemDetails">当前选中的物品数据</param>
    private void OnMouseClickEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        //如果需要执行对应的动画
        if (itemDetails.itemType != ItemType.Seed
            && itemDetails.itemType != ItemType.Furniture
            && itemDetails.itemType != ItemType.Commodity)
        {
            //鼠标位置距离玩家的差值
            mouseX = mouseWorldPos.x - transform.position.x;
            mouseY = mouseWorldPos.y - (transform.position.y+0.85f);
            //斜方向判断 
            //横向差值大于纵向差值
            if (MathF.Abs(mouseX) > MathF.Abs(mouseY))
                mouseY = 0;
            //横向差值小于于纵向差值
            else
                mouseX = 0;

            StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));

        }
        //如果不需要执行动画
        else
        {
            //TODO:执行完动画之后的逻辑
            /* 主角完成一个动作的动画结束后执行
             * 1、在地图上实现实际工具使用或种植 仍物品物品等相关功能
             *    在这里是执行除工具外的丢弃 放置家具 等不需要先执行动画的相关的内容
             *
             */
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos,itemDetails);
        }
        
        
    }
    private void OnUpdateGameStateEvent(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.GamePlay:
                inputDisable = false;
                break;
            case GameState.Pause:
                inputDisable = true;
                break;
        }
    }
    #endregion
    
    #region 主角移动相关方法
    /// <summary>
    /// 根据x轴和y轴的输入得到Player移动的二维向量
    /// </summary>
    private void PlayerInput()
    {
        //OPTIMIZE:没有移动输入的时候不需要执行这么多逻辑
        //获取x轴和y轴的输入
        // if (inputY == 0) 
        inputX = Input.GetAxis("Horizontal");
        // if (inputX == 0) 
        inputY = Input.GetAxis("Vertical");
        if (inputX != 0 && inputY != 0)
        {
            inputX = inputX * 0.6f;
            inputY = inputY * 0.6f;
            
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            inputX *= 0.5f;
            inputY *= 0.5f;
        }
        movementInput = new Vector2(inputX, inputY);
        //更新是否在移动
        isMoving = movementInput != Vector2.zero;
    }
    
    /// <summary>
    /// player移动的方法
    /// </summary>
    private void Movement()
    {
        //OPTIMIZE: 没有移动输入的时候不需要移动的方法
        //移动
        rb.MovePosition(rb.position + movementInput * (speed * Time.deltaTime));
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mouseWorldPos"></param>
    /// <param name="itemDetails"></param>
    /// <returns></returns>
    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        useTool = true;
        inputDisable = true;
        yield return null;
        foreach (var anim in animators)
        {
            anim.SetTrigger(Settings.AnimTriggerParaUseTool);
            //更改人物转动方向
            anim.SetFloat(Settings.AnimFloatParameterInputX,mouseX);
            anim.SetFloat(Settings.AnimFloatParameterInputY,mouseY);
        }
        yield return new WaitForSeconds(0.45f);
        /* 主角完成一个动作的动画结束后执行
         * 1、在地图上实现实际工具使用或种植 仍物品物品等相关功能
         *    在这里是执行的是实用工具需要先有动画在有逻辑的部分
         *
         */
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos,itemDetails);
        yield return new WaitForSeconds(0.25f);
        useTool = false;
        inputDisable = false;

    }
    #region 动画相关方法
    
   
    /// <summary>
    /// 切换动画
    /// </summary>
    private void SwitchAnimation()
    {
        foreach (var animator in animators)
        {
            animator.SetBool(Settings.AnimBoolParameterIsMoving,isMoving);
            animator.SetFloat(Settings.AnimFloatParameterMouseX,mouseX);
            animator.SetFloat(Settings.AnimFloatParameterMouseY,mouseY);
            if (isMoving)
            {
                animator.SetFloat(Settings.AnimFloatParameterInputX,inputX);
                animator.SetFloat(Settings.AnimFloatParameterInputY,inputY);
            }
        }
    }
    #endregion

    #region 保存相关
    //获取guid
    public string Guid => GetComponent<DataGUID>().guid;
    public GameSaveData GenerateSaveData()
    {
        //创建字典
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        //保存玩家的坐标
        saveData.characterPosDict.Add(this.name,new SerializableVector3(transform.position));
        return saveData;
    }

    public void RestoreData(GameSaveData gameSaveData)
    {
        //设置坐标
        // transform.position = gameSaveData.characterPosDict[this.name].ToVector3();
        StartCoroutine(Restore(gameSaveData.characterPosDict[this.name].ToVector3()));
    }

    private IEnumerator Restore(Vector3 pos)
    {
        yield return new WaitForSeconds(Settings.fadeDuration);
        transform.position = pos;
    }
    #endregion
    
    
}
