using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LYFarm.Transition
{
    public class Teleport : MonoBehaviour
    {
        [SceneName]
        public string sceneName;
        public Vector3 positionToGo;



        private void OnTriggerEnter2D(Collider2D other)
        {
            
            if (other.CompareTag("Player"))
            {   
                EventHandler.CallTransitionEvent(sceneName,positionToGo);
            }
        }
    }
}

