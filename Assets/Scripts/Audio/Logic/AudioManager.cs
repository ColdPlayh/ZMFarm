
using System.Collections;
using System.Collections.Generic;
using Audio.Data;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


/// <summary>
/// 音频管理者
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
   [Header("音乐数据库")]
   public SoundDetailsList_SO soundDetailsData;
   public SceneSoundList_SO sceneSoundData;

   [Header("AUdio Source")] 
   public AudioSource ambientSource;
   public AudioSource gameSource;

   private Coroutine soundRoutine;
   [Header("Audio Mixer")]
   public AudioMixer audioMixer;
   [Header("SnapShort")] 
   public AudioMixerSnapshot normalSnapShot;
   public AudioMixerSnapshot ambientOnlySnapShot;
   public AudioMixerSnapshot muteSnapShot;
   public float MusicStartSecond => Random.Range(5f, 15f);

   public bool musicPause;
   //snapshort的转换时间
   private float musicTransitionSecond=8f;

   private void OnEnable()
   {
      EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
      EventHandler.PlaySoundEvent += OnPlaySoundEvent;
      EventHandler.EndGameEvent += OnEndGameEvent;
   }

   private void OnDisable()
   {
      EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
      EventHandler.PlaySoundEvent -= OnPlaySoundEvent;
      EventHandler.EndGameEvent -= OnEndGameEvent;
   }

   private void OnEndGameEvent()
   {
      if (soundRoutine is not null)
      {
         StopCoroutine(soundRoutine);
      }
      muteSnapShot.TransitionTo(1);
      
   }

   //播放声音
   private void OnPlaySoundEvent(SoundName soundName)
   {
      //通过soundName得到对应的sound的数据
      var soundDetails = soundDetailsData.GetSoundDetails(soundName);
      if (soundDetails is not null)
      {
         //初始化sound
         EventHandler.CallInitSoundEffect(soundDetails);
      }
   }

   //重新加载场景后调用的事件方法
   public void OnAfterSceneLoadedEvent()
   {

      if (musicPause) return;
      //获取当前场景的音乐
      string currSceneName = SceneManager.GetActiveScene().name;
      SceneSoundItem sceneSoundItem = sceneSoundData.GetSceneSoundDetails(currSceneName);
      if (sceneSoundItem is null) return;
      //获取音频数据
      SoundDetails ambient = soundDetailsData.GetSoundDetails(sceneSoundItem.ambient);
      SoundDetails music = soundDetailsData.GetSoundDetails(sceneSoundItem.music);
      //如果携程已经开始则关闭携程
      if (soundRoutine is not null)
      {
         StopCoroutine(soundRoutine);
      }
      //启动播放音乐的携程
      soundRoutine = StartCoroutine(PlaySoundRoutinue(music, ambient));
      
   }

   //播放音乐
   private IEnumerator PlaySoundRoutinue(SoundDetails music,SoundDetails ambient)
   {
      //背景音和音乐存在
      if (music is not null && ambient is not null)
      {
         //先播放背景音
         PlayAmbientClip(ambient,1f);
         yield return new WaitForSeconds(MusicStartSecond);
         //在播放音乐
         PlayMusicClip(music,musicTransitionSecond);
      }
         
   }

   //播放音乐
   public void PlayMusicClip(SoundDetails soundDetails,float transitionTime)
   {
      //设置Audiomixer的音量
      audioMixer.SetFloat("MusicVolume", ConvertSoundVolume(soundDetails.soundVolume));
      
      //播放背景音
      gameSource.clip = soundDetails.audioClip;
      if (gameSource.isActiveAndEnabled)
      {
         gameSource.Play();
      }
      normalSnapShot.TransitionTo(transitionTime);
   }
   //播放背景音
   public void PlayAmbientClip(SoundDetails soundDetails,float transitionTime)
   {
      audioMixer.SetFloat("AmbientVolume", ConvertSoundVolume(soundDetails.soundVolume));
      //播放背景音
      ambientSource.clip = soundDetails.audioClip;
      if (ambientSource.isActiveAndEnabled)
      {
         ambientSource.Play();
      }
      ambientOnlySnapShot.TransitionTo(transitionTime);
   }

   //将我们设置的volume转化为audiomixer中db
   private float ConvertSoundVolume(float volume)
   {
      return volume * 100 - 80;
   }

   public void SetMasterVolume(float volume)
   {
      audioMixer.SetFloat("MasterVolume", volume*100-80);
   }
   
}
