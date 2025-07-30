using Audio.Data;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class Sound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public void SetSound(SoundDetails soundDetails)
    {
        //设置音频
        audioSource.clip = soundDetails.audioClip;
        //设置音量
        audioSource.volume = soundDetails.soundVolume;
        //随机设置音调
        audioSource.pitch = Random.Range(soundDetails.soundPitchMin, soundDetails.soundPitchMax);
        
    }
}