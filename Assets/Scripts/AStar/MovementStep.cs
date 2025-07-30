using UnityEngine;

namespace LYFarm.AStarN
{
    public class MovementStep
    {
        //步骤对应的场景
        public string sceneName;
        //步骤对应的时分秒
        public int hour, minute, second;
        /// <summary>
        /// 步骤对应的坐标 (场景中真实的坐标)
        /// </summary>
        public Vector2Int gridCoordinate;
    }
}