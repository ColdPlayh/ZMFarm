using System;
using Cinemachine;
using UnityEngine;


namespace LYFarm.Camera
{
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
        [HideInInspector] public CinemachineFramingTransposer cinemachineFramingTransposer;


        private void Start()
        {
            if (cinemachineVirtualCamera)
            {
                cinemachineFramingTransposer =
                    cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        public void AfterFadeOut()
        {
            if (cinemachineFramingTransposer)
            {
                cinemachineFramingTransposer.m_XDamping = 0;
                cinemachineFramingTransposer.m_YDamping = 0;
            }
        }

        public void AfterFadeIn()
        {
            if (cinemachineFramingTransposer)
            {
                cinemachineFramingTransposer.m_XDamping = 1;
                cinemachineFramingTransposer.m_YDamping = 1;
            }
        }
    }
}