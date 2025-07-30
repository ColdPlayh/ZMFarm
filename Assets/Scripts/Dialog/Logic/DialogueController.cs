using System;
using System.Collections;
using System.Collections.Generic;
using LYFarm.NPC;
using UnityEngine;
using UnityEngine.Events;

namespace LYFarm.Dialogue
{
    [RequireComponent(typeof(NPCMovement))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour
    {
        private NPCMovement npcMovement;

        public UnityEvent onFinshEvent;
        public List<DialoguePiece> dialoguePieces = new List<DialoguePiece>();


        private Stack<DialoguePiece> dialoueStack;

        private bool canTalk;

        private bool isTalking;
        private GameObject uiSign;
        private void Awake()
        {
            npcMovement = GetComponent<NPCMovement>();
            uiSign = transform.GetChild(1).gameObject;
            
            FillDialogueStack();
        }

        private void Update()
        {
            //更新提示ui
            uiSign.SetActive(canTalk);

            //显示对话ui
            if (canTalk && !isTalking && Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(DialoguerRoutine());
            }
            
        }

        private IEnumerator DialoguerRoutine()
        {
            //正在对话
            isTalking = true;
            //
            if (dialoueStack.TryPop(out DialoguePiece result))
            {
                //显示对话
                EventHandler.CallShowDialogueEvent(result);
                //暂停玩家的输入
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                //显示对话
                yield return new WaitUntil(() => result.isDone);
                isTalking = false;
            }
            else
            {
                EventHandler.CallShowDialogueEvent(null);
                FillDialogueStack();
                isTalking = false;
                EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
                if (onFinshEvent != null)
                {
                    onFinshEvent?.Invoke();
                    canTalk = false;
                }
               
                
                //对话结束的回调方法
                
            }
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log(npcMovement.isMoving+":"+npcMovement.isInteractable);
                //npc没有移动并且此时可以互动
                canTalk = !npcMovement.isMoving && npcMovement.isInteractable;
                
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                //npc没有移动并且此时可以互动
                canTalk = false;

            }
        }

        private void FillDialogueStack()
        {
            dialoueStack = new Stack<DialoguePiece>();
            for (int i = dialoguePieces.Count-1; i >=0; i--)
            {
                dialoguePieces[i].isDone = false;
                dialoueStack.Push(dialoguePieces[i]);
            }
        }
    }
}