using System.Collections.Generic;
using UnityEngine;

namespace NPC.Data
{
    [CreateAssetMenu(fileName = "SceneRouteDataList_SO", menuName = "NPC/SceneRouteDataList", order = 1)]
    public class SceneRouteDataList_SO : ScriptableObject
    {
        public List<SceneRoute> sceneRouteList;
    }
}