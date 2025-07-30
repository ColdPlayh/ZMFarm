using System;
using System.Collections;
using System.Collections.Generic;
using Audio.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    #region 字段
    
    public List<GameObject> poolPrefabs;
    //OPTIMIZE 使用字典更好
    private List<ObjectPool<GameObject>> poolEffectList = new List<ObjectPool<GameObject>>();
    private Queue<GameObject> soundQueue = new Queue<GameObject>();
    #endregion
   

    #region 生命周期方法
    
    private void OnEnable()
    {
        EventHandler.ParticleEffectEvent += OnParticleEffectEvent;
        EventHandler.InitSoundEffect += InitSoundEffect;
    }

    private void Start()
    {
        CreatePool();
    }

    private void OnDisable()
    {
        EventHandler.ParticleEffectEvent -= OnParticleEffectEvent;
        EventHandler.InitSoundEffect -= InitSoundEffect;
    }

 

    #endregion

    #region 事件方法
    
    private void OnParticleEffectEvent(ParticleEffectType effectType, Vector3 pos)
    {
        var objPool = effectType switch
        {
            ParticleEffectType.LeavesFalling01 => poolEffectList[0],
            ParticleEffectType.LeavesFalling02 => poolEffectList[1],
            ParticleEffectType.Rock=>poolEffectList[2],
            ParticleEffectType.ReapableScenery=>poolEffectList[3],
            _ => null
        };
        GameObject obj = objPool.Get();
        obj.transform.position = pos;
        StartCoroutine(ReleaseRoutine(objPool, obj));
    }
    
    #endregion
    
    #region 功能方法
    
    //初始化对象池
    private void CreatePool()
    {
        //遍历需要存入对象池的list
        foreach (GameObject item  in poolPrefabs)
        {
            //创建一个新的父物体 每一种类型的物体放在一个父物体下
            Transform parent = new GameObject(item.name).transform;
            parent.SetParent(transform);
            //创建对应物体的对象池
            var newPool = new ObjectPool<GameObject>(
                () => Instantiate(item, parent),
                e => { e.SetActive(true); },
                e => { e.SetActive(false); },
                e => { Destroy(e); }
            );
            //初始化对象池列表
            poolEffectList.Add(newPool);
        }
    }

    private IEnumerator ReleaseRoutine(ObjectPool<GameObject> objPool,GameObject obj)
    {
        yield return new WaitForSeconds(1.5f);
        objPool.Release(obj);
    }

    //播放声音
    // private void InitSoundEffect(SoundDetails soundDetails)
    // {
    //     ObjectPool<GameObject> soundPool = poolEffectList[4];
    //     var sound = soundPool.Get();
    //     sound.GetComponent<Sound>().SetSound(soundDetails);
    //     StartCoroutine(DisableSound(soundPool, sound, soundDetails));
    //
    // }
    //
    // //释放声音
    // private IEnumerator DisableSound(ObjectPool<GameObject> objPool, GameObject obj,SoundDetails soundDetails)
    // {
    //     yield return new WaitForSeconds(soundDetails.audioClip.length);
    //     objPool.Release(obj);
    // }
    #endregion
    
    #region 简易的音乐对象池
    //创建音乐pool
    private void CreateSoundPool()
    {
        var parent = new GameObject(poolPrefabs[4].name).transform;
        parent.SetParent(transform);
        for (int i = 0; i < 20; i++)
        {
            GameObject newObj = Instantiate(poolPrefabs[4],parent);
            newObj.SetActive(false);
            soundQueue.Enqueue(newObj);
        }
    }

    //获取对象
    private GameObject GetPoolObject()
    {
        if (soundQueue.Count < 2)
        {
            CreateSoundPool();
        }

        return soundQueue.Dequeue();
    }

    //初始化一个sound的方法
    private void InitSoundEffect(SoundDetails soundDetails)
    {
        GameObject sound= GetPoolObject();
        //设置sound
        sound.GetComponent<Sound>().SetSound(soundDetails);
        //因为sound的prefab设置为play on awake 所以只要setactive true就会播放
        sound.SetActive(true);
        StartCoroutine(DisableSound(sound, soundDetails));

    }

    //释放资源的携程
    private IEnumerator DisableSound(GameObject obj,SoundDetails soundDetails)
    {
        yield return new WaitForSeconds(soundDetails.audioClip.length);
        obj.SetActive(false);
        soundQueue.Enqueue(obj);
    }
    #endregion
    
}
