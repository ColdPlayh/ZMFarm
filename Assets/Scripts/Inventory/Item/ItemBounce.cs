using System;
using UnityEngine;
using LYFarm.Inventory;

public class ItemBounce : MonoBehaviour
{
    private Transform spriteTrans;

    private BoxCollider2D coll;

    //重力
    public float gravity=-4f;
    //是否落地
    private bool isGround;
    //与目标的距离
    private float distance;
    //方向
    private Vector2 direction;
    //目标位置
    private Vector3 targetPos;
    private bool isBounce;
    private void Awake()
    {
        spriteTrans = transform.GetChild(0);
        coll = GetComponent<BoxCollider2D>();
        
    }

    private void Start()
    {
        //设置触发器无效 防止刚丢出去玩家就捡起来了
        coll.enabled = false;
    }

    private void Update()
    {
        //未完成一次丢出
        if (!isBounce) 
            Bounce();
    }

    //初始化
    public void InitBounceItem(Vector3 target,Vector2 dir)
    {
        //关闭触发器
        coll.enabled = false;
        //设置方向和目标
        direction = dir;
        targetPos = target;
        //计算距离
        distance = Vector3.Distance(transform.position, target);
        //设置初始的扔出位置
        spriteTrans.position+=Vector3.up*1.5f;
    }
    
    // OPTIMIZE:优化物品扔出的动画算法 以及落地后不需要在执行此方法的逻辑
    private void Bounce()
    {
        
        //判断是否落地
        isGround = spriteTrans.position.y <= transform.position.y;
        //x轴方向的位移
        if(Vector3.Distance(transform.position,targetPos)>0.1f)
        {
            transform.position += (Vector3) direction*distance*-gravity*Time.deltaTime;
        }
        //未落地之前y轴的位移
        if (!isGround)
        {
            spriteTrans.position += Vector3.up * gravity * Time.deltaTime;
        }
        //落地后修复一下位置 并设置触发器可用
        else
        {
            spriteTrans.transform.position= transform.position+Vector3.down*0.01f;
            coll.enabled = true;
            //已经丢出 不需要再执行此方法
            isBounce = true;
        }
    }
}
