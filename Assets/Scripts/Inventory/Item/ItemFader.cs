using DG.Tweening;
using UnityEngine;

//需要有SpriteRenderer组件
[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 逐渐恢复颜色
    /// </summary>
    public void FadeIn()
    {
        Color targetColor = new Color(1, 1, 1, 1);
        spriteRenderer.DOColor(targetColor, Settings.ItemFadeDuration);
    }

    /// <summary>
    /// 逐渐半透明
    /// </summary>
    public void FadeOut()
    {
        Color targetColor = new Color(1, 1, 1, Settings.TargetAlpha);
        spriteRenderer.DOColor(targetColor, Settings.ItemFadeDuration);
    }
}


