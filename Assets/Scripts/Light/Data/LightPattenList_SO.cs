using System;
using System.Collections.Generic;
using UnityEngine;

namespace Light.Data
{
    [CreateAssetMenu(fileName = "LightPattenList_SO", menuName = "Light/ Light Patten")]
    public class LightPattenList_SO : ScriptableObject
    {
        public List<LightDetails> lightDetailsList;

        /// <summary>
        /// 获取光源数据
        /// </summary>
        /// <param name="season">季节</param>
        /// <param name="lightShift"></param>
        /// <returns></returns>
        public LightDetails GetLightDetails(Season season, LightShift lightShift)
        {
            return lightDetailsList.Find(l => l.Season == season && l.lightShift == lightShift);
        }
    }

    [Serializable]
    public class LightDetails
    {
        public Season Season;
        public Color lightColor;
        public float LightAmount;
        public LightShift lightShift;

    }
}