using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInterActive : MonoBehaviour
{
   public bool isAnimating;
   private WaitForSeconds pause = new WaitForSeconds(0.04f);
   private int rotateCount = 4;
   private int singleShakeAngle = 2;

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.H))
      {
         EventHandler.CallPlaySoundEvent(SoundName.Rustle);
      }
   }

   private void OnTriggerEnter2D(Collider2D other)
   {
      if (!isAnimating)
      {
         //玩家在左侧进入
         if (other.transform.position.x < transform.position.x)
         {
            //向右摇晃
            StartCoroutine(RotateRight());
         }
         else
         {
            //向左摇晃
            StartCoroutine(RotateLeft());
         }
         EventHandler.CallPlaySoundEvent(SoundName.Rustle);
         
      }
   }

   private void OnTriggerExit2D(Collider2D other)
   {
      if (!isAnimating)
      {
        
         //玩家在左侧离开
         if (other.transform.position.x < transform.position.x)
         {
            //向左摇晃
            
            StartCoroutine(RotateLeft());
         }
         else
         {
            //向右摇晃
            StartCoroutine(RotateRight());
         }
         EventHandler.CallPlaySoundEvent(SoundName.Rustle);
         
      }
   }

   private IEnumerator RotateLeft()
   {
      isAnimating = true;
      for (int i = 0; i < rotateCount; i++)
      {
         transform.GetChild(0).Rotate(0,0,singleShakeAngle);
         yield return pause;
      }

      for (int i = 0; i < rotateCount+1; i++)
      {
         transform.GetChild(0).Rotate(0,0,-singleShakeAngle);
         yield return pause;
      }
      transform.GetChild(0).Rotate(0,0,singleShakeAngle);
      yield return pause;
      isAnimating = false;
   }
   private IEnumerator RotateRight()
   {
      isAnimating = true;
      for (int i = 0; i < rotateCount; i++)
      {
         transform.GetChild(0).Rotate(0,0,-singleShakeAngle);
         yield return pause;
      }

      for (int i = 0; i < rotateCount+1; i++)
      {
         transform.GetChild(0).Rotate(0,0,singleShakeAngle);
         yield return pause;
      }
      transform.GetChild(0).Rotate(0,0,-singleShakeAngle);
      yield return pause;
      isAnimating = false;
   }
}
