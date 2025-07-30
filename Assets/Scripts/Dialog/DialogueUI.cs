using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


namespace LYFarm.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        public GameObject dialogBox;
        public Text dialogText;
        public Image faceRight, faceLeft;
        public Text nameRight, nameLeft;

        public GameObject continueBox;

        private void Awake()
        {
            continueBox.SetActive(false);
        }

        private void OnEnable()
        {
            EventHandler.ShowDialogueEvent += OnShowDialogueEvent;
        }

        private void OnDisable()
        {
            EventHandler.ShowDialogueEvent -= OnShowDialogueEvent;
        }

        private void OnShowDialogueEvent(DialoguePiece piece)
        {
            
            StartCoroutine(ShowDialogue(piece));
        }

        private IEnumerator ShowDialogue(DialoguePiece piece)
        {
            
            if (piece is not null)
            {
               
                piece.isDone = false;
                dialogBox.SetActive(true);
                continueBox.SetActive(false);
                
                dialogText.text=String.Empty;
                if (!string.IsNullOrEmpty(piece.name))
                {
                    if (piece.onLeft)
                    {
                        faceLeft.gameObject.SetActive(true);
                        faceRight.gameObject.SetActive(false);
                        faceLeft.sprite = piece.faceSprite;
                        nameLeft.text = piece.name;
                    }
                    else
                    {
                        
                        faceLeft.gameObject.SetActive(false);
                        faceRight.gameObject.SetActive(true);
                        faceRight.sprite = piece.faceSprite;
                        nameRight.text = piece.name;
                    }
                }
                else
                {
                    faceLeft.gameObject.SetActive(false);
                    faceRight.gameObject.SetActive(false);
                    nameLeft.gameObject.SetActive(false);
                    nameRight.gameObject.SetActive(false);
                }
                
                yield return dialogText.DOText(piece.dialogText, 1f).WaitForCompletion();

                piece.isDone = true;

                if (piece.hasToPause && piece.isDone)
                    continueBox.SetActive(true);
            }
            else
            {
                dialogBox.SetActive(false);
                yield break;
            }
        }
        
    }
}