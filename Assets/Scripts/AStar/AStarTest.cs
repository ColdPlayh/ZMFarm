using System;
using System.Collections;
using System.Collections.Generic;
using LYFarm.NPC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;


namespace LYFarm.AStarN
{
    public class AStarTest : MonoBehaviour
    {
        private AStar _aStar;
        [Header("测试位置")]
        public Vector2Int startPos;
        // public Vector2Int targetPos;

        //用于显示的tile层
        public Tilemap displayMap;
        //用于测试的瓦片
        public TileBase displayTile;

        public TileBase pathTile;

        public bool displayStartAndFinish;
        public bool displayPath;

        private Stack<MovementStep> npcMovementStepStack;
        [Header("测试移动npc")] public NPCMovement mNpcMovement;
        public bool moveNpc;

        [SceneName] public string targetSceneName;

        public Vector2Int targetPos;

        public AnimationClip stopClip;

        [Header("测试astar")] public bool isGenerate;
        
        

        private void Awake()
        {
            _aStar = GetComponent<AStar>();
            npcMovementStepStack = new Stack<MovementStep>();
        }

        private void Update()
        {
            ShowPathOnGridMap();

            if (moveNpc)
            {
                moveNpc = false;
                Debug.Log("astar start");
                var schedule = new ScheduleDetails
                    (0, 0, 0, 0, Season.春天, targetSceneName, targetPos, stopClip,true);
                mNpcMovement.BuildPath(schedule);
            }
            
            if(isGenerate)
                GenerateTest();
                
        }
        
        private void ShowPathOnGridMap()
        {
            if (displayMap is not null && displayTile is not null)
            {
                if (displayStartAndFinish)
                {
                    displayMap.SetTile((Vector3Int)startPos,displayTile);
                    displayMap.SetTile((Vector3Int)targetPos,displayTile);
                }
                else
                {
                    displayMap.SetTile((Vector3Int)startPos,null);
                    displayMap.SetTile((Vector3Int)targetPos,null);
                }

                if (displayPath)
                {
                    var sceneName = SceneManager.GetActiveScene().name;
                    _aStar.BuildPath(sceneName,startPos,targetPos,npcMovementStepStack);
                    foreach (var step in npcMovementStepStack)
                    {
                        displayMap.SetTile((Vector3Int)step.gridCoordinate,pathTile);
                    }
                }
                else
                {
                    foreach (var step in npcMovementStepStack)
                    {
                        displayMap.SetTile((Vector3Int)step.gridCoordinate,null);
                    }
                    npcMovementStepStack.Clear();
                }
            }
        }

        private void GenerateTest()
        {
            _aStar.GenerateGridNodes(targetSceneName, startPos, targetPos);
        }
    }
}

