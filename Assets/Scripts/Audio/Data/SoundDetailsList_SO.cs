using System;
using System.Collections.Generic;
using UnityEngine;

namespace Audio.Data
{
    [CreateAssetMenu(fileName = "SoundSoundDetailsList_SO", menuName = "Sound/SoundDetailsList", order = 0)]
    public class SoundDetailsList_SO : ScriptableObject
    {
        public List<SoundDetails> soundDetailsList;

        public SoundDetails GetSoundDetails(SoundName soundName)
        {
            return soundDetailsList.Find(s => s.soundName == soundName);
        }
        
    }

    [Serializable]
    public class SoundDetails
    {
        public SoundName soundName;
        public AudioClip audioClip;
        [Range(0.1f,1.5f)]
        public float soundPitchMin = 0.8f;
        [Range(0.1f,1.5f)]
        public float soundPitchMax = 1.2f;
        [Range(0.1f,1f)]
        public float soundVolume = 0.2f;

    }
}