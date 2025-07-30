using System;
using UnityEngine;

namespace LYFarm.AStarN
{
    public class Node : IComparable<Node>
    {
        //节点坐标
        public Vector2Int girdPosition;

        public Vector2Int worldPos;
        //距离起点的距离（）
        public int gCost;
        //距离目标点的距离
        public int hCost;
        //预购代价
        public int FCost => gCost + hCost;
        //当前格子是否是障碍
        public bool isObstacle;
        public Node parentNode;
        
        public Node(Vector2Int pos)
        {
            girdPosition = pos;
            parentNode = null;
        }
        
        public int CompareTo(Node other)
        {
            int result = FCost.CompareTo(other.FCost);
            if (result == 0)
            {
                result = hCost.CompareTo(other.hCost);
            }

            return result;
        }
    }
}