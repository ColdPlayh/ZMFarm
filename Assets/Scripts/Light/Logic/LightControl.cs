using System;
using DG.Tweening;
using Light.Data;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class LightControl : MonoBehaviour
{
    public LightPattenList_SO lightPattenListSo;
    public Light2D currentLight;
    private LightDetails currentLightDetails;

    private void Awake()
    {
        currentLight = GetComponent<Light2D>();
    }
    
    //实际切换灯光
    public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
    {
        //获取当前季节和时间对应的灯光
        currentLightDetails = lightPattenListSo.GetLightDetails(season, lightShift);
        //有足够的时间变化就逐步变化
        if (timeDifference < Settings.lightChangeDuration)
        {
            //获取差值
            var colorOffset = (currentLightDetails.lightColor - currentLight.color)/Settings.lightChangeDuration *timeDifference;
            currentLight.color += colorOffset;
            //dotween进行差值
            DOTween.To(() => currentLight.color,
                c => currentLight.color = c, 
                currentLightDetails.lightColor,
                Settings.lightChangeDuration-timeDifference);
            DOTween.To(() => currentLight.intensity,
                i => currentLight.intensity = i,
                currentLightDetails.LightAmount,
                Settings.lightChangeDuration - timeDifference);
            
        }
        //没有足够的时间变化直接变化
        else if (timeDifference >= Settings.lightChangeDuration)
        {
            currentLight.color = currentLightDetails.lightColor;
            currentLight.intensity = currentLightDetails.LightAmount;
        }
            
        
    }
}
