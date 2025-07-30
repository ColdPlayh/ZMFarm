using System;
using System.Collections.Generic;
using UnityEngine;
using LYFarm.Map;
using UnityEngine.Tilemaps;

namespace LYFarm.AStarN
{
    public class AStar : Singleton<AStar>
    {
        private GridNodes gridNodes;
        private Node starNode;
        private Node targetNode;
        private int gridWidth;
        private int gridHeight;
        private int originalX;
        private int originalY;
        /// <summary>
        /// 当前选中节点的周围的节点
        /// </summary>
        private List<Node> openNodeList;
        /// <summary>
        /// 所有被选中的点
        /// </summary>
        private HashSet<Node> closedNodeList;
        

        private bool pathFound;
        [Header("测试")]

        public Tilemap tiles;

        public TileBase tileBase;

       

        /// <summary>
        /// 生成路径
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="startPos">开始位置</param>
        /// <param name="targetPos">终点位置</param>
        /// <param name="npcMovementStack"></param>
        public void BuildPath(string sceneName, Vector2Int startPos, Vector2Int targetPos,
            Stack<MovementStep> npcMovementStack)
        {
            pathFound = false;
            if (GenerateGridNodes(sceneName, startPos, targetPos))
            {
                // Debug.Log("ce 成功生成node");
                if (FindShortestPath())
                {
                    // Debug.Log("ce 找到最短路径");
                    //构建路径
                    UpdatePathOnMovementStepStack(sceneName,npcMovementStack);
                    // Debug.Log("ce 更新stack");
                }
            }
        }
        /// <summary>
        /// 构建网格节点信息 初始化两个列表
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="startPos">起始点</param>
        /// <param name="targetPos">终止点</param>
        /// <returns></returns>
        public bool GenerateGridNodes(string sceneName,Vector2Int startPos,Vector2Int targetPos)
        {
            //TODO 如果目标点超出范围则返回false
            
            if (GirdManager.Instance.GetGridDimensions(sceneName,
                    out Vector2Int gridDimensions,
                    out Vector2Int gridOriginal))
            {
                //根据地图范围和起始点信息生成a星节点数组
                gridWidth = gridDimensions.x;
                gridHeight = gridDimensions.y;
                originalX = gridOriginal.x;
                originalY = gridOriginal.y;
                
                //初始化节点信息
                gridNodes = new GridNodes(gridWidth,gridHeight);

                openNodeList = new List<Node>();
                closedNodeList = new HashSet<Node>();
            }
            else
            {
                return false;
            }
            //gridNode是从0，0开始的 需要进行坐标转换 
            starNode = gridNodes.GetGridNode(startPos.x-originalX, startPos.y-originalY);
            targetNode = gridNodes.GetGridNode(targetPos.x - originalX, targetPos.y - originalY);
            //初始化每一个node
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var key =(x+originalX)+"x"+(y+originalY) +"y" + sceneName;
                    TileDetails tile = GirdManager.Instance.GetTileDetails(key);
                    if (tile is not null)
                    {
                        Node node = gridNodes.GetGridNode(x, y);
                        if (tile.isNpcObstacle)
                        {
                            
                            node.isObstacle = true;
                            // tiles.SetTile(new Vector3Int(node.girdPosition.x+originalX,node.girdPosition.y+originalY,0),tileBase);
                        }
                        

                    }
                    
                }
            }
            
           
            // Debug.Log("测试");
            return true;
        }

        /// <summary>
        /// 寻找是否最短路径
        /// </summary>
        /// <returns></returns>
        public bool FindShortestPath()
        {
            
            openNodeList.Add(starNode);
            // Debug.Log("ce startnode"+gridToWorld(starNode.girdPosition));
            //只要存在探测的节点
            while (openNodeList.Count > 0)
            {
                //最小的在第一个 或者说代价最小的节点
                openNodeList.Sort();
                //拿出代价最小的点进行探测
                Node closeNode = openNodeList[0];
                openNodeList.RemoveAt(0);
                //添加到已经选择过的点
                closedNodeList.Add(closeNode);
                // Debug.Log("ce closenode"+closeNode.girdPosition+":"+targetNode.girdPosition);
                //如果到达目的地则搜寻成功
                if (closeNode == targetNode)
                {
                    pathFound = true;
                    break;
                }
               
                //评估周围的点
                //
                EvaluateNeighbourNodes(closeNode);

            }
            
            return pathFound;
        }
        
        /// <summary>
        /// 评估周围八个点的代价
        /// </summary>
        /// <param name="currentNode"></param>
        public void EvaluateNeighbourNodes(Node currentNode)
        {
            Vector2Int currentNodePos = currentNode.girdPosition;
            Node validNeighbourNode;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    //不评估自己
                    if(x==0 && y==0) continue;

                    //获取评估的节点
                    validNeighbourNode = GetVaildNeighbourNode(currentNodePos.x+x, currentNodePos.y+y);
                    //如果节点无效跳过
                    if (validNeighbourNode is null) continue;
                    //如果没有放在评估的节点中
                    if (!openNodeList.Contains(validNeighbourNode))
                    {
                        //计算代价设置父节点
                        //gcost和hcost
                        validNeighbourNode.gCost = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);
                        validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);
                        validNeighbourNode.parentNode = currentNode;
                        openNodeList.Add(validNeighbourNode);
                    }

                }
            }
        }

        /// <summary>
        /// 计算代价 其中斜方向为14
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Node GetVaildNeighbourNode(int x, int y)
        {
            //超出边界返回空
            if (x >= gridWidth || x < 0 || y >= gridHeight || y < 0) 
                return null;
            //获取这个节点
            Node NeighbourNode = gridNodes.GetGridNode(x, y);
            //如果是障碍 或者已经选择过的节点则返回null
            if (NeighbourNode.isObstacle || closedNodeList.Contains(NeighbourNode))
            {
                return null;
            }
            //返回节点
            return NeighbourNode;
            

        }

        /// <summary>
        /// 判断两个点的距离值
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>
        private int GetDistance(Node nodeA,Node nodeB)
        {
            int xDistance = Mathf.Abs(nodeA.girdPosition.x - nodeB.girdPosition.x);
            int yDistance = Math.Abs(nodeA.girdPosition.y - nodeB.girdPosition.y);
            if (xDistance > yDistance) 
                return 14 * yDistance+10*(xDistance-yDistance);
            return 14 * xDistance+10*(yDistance-xDistance);
        }

        private void UpdatePathOnMovementStepStack(string sceneName,Stack<MovementStep> movementStepStack)
        {
            Node nextNode = targetNode;
            while (nextNode is not null)
            {
                MovementStep newStep = new MovementStep();
                newStep.sceneName = sceneName;
                //获取tile坐标
                newStep.gridCoordinate = new Vector2Int(nextNode.girdPosition.x + originalX,
                    nextNode.girdPosition.y + originalY);
                //压入堆栈
                movementStepStack.Push(newStep);
                nextNode = nextNode.parentNode;
            }
        }

        private Vector2Int gridToWorld(Vector2Int grid)
        {
            return new Vector2Int(grid.x + originalX, grid.y + originalY);
        }
        
    }  
}