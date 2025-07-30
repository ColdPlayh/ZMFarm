using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LYFarm.Dialogue
{
    [Serializable]
    public class DialoguePiece
    {
        [Header("对话详情")] public Sprite faceSprite;

        public bool onLeft;
        public string name;
        [TextArea] public string dialogText;

        public bool hasToPause;

        [HideInInspector]
        public bool isDone;

        public UnityEvent afterTalkEvent;
    }
}