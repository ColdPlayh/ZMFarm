using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 主角与物体遮挡剔除
/// </summary>
public class TriggerItemFader : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();
        if (faders.Length > 0)
        {
            Debug.Log("---------------------------------");
            Debug.Log(faders.Length);
            foreach (var item in faders)
            {
              //  Debug.Log(item);
                item.FadeOut();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();
        if (faders.Length > 0)
        {
            //Debug.Log("---------------------------------");
           // Debug.Log(faders.Length);
            foreach (var item in faders)
            {
               // Debug.Log(item);
                item.FadeIn();
            }
        }
    }
}
