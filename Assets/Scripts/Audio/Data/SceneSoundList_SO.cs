using System;
using System.Collections.Generic;
using UnityEngine;

namespace Audio.Data
{
    [CreateAssetMenu(fileName = "SceneSoundList_SO", menuName = "Sound/SceneSoundList")]
    public class SceneSoundList_SO : ScriptableObject
    {
        public List<SceneSoundItem> sceneSoundDetailsList;

        public SceneSoundItem GetSceneSoundDetails(string sceneName)
        {
            return sceneSoundDetailsList.Find(s => s.sceneName == sceneName);
        }
    }

    [Serializable]
    public class SceneSoundItem
    {
        [SceneName]
        public string sceneName;
        public SoundName ambient;
        public SoundName music;
        



    }
}