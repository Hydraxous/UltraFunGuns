using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class UltraFunGunBase : MonoBehaviour
    {
        public float fireDelayPrimary = 0.01f, fireDelaySecondary = 0.01f;
        public float timeToFirePrimary = 0.0f, timeToFireSecondary = 0.0f;

        public Transform mainCam, firePoint;

        public bool noCooldown = false;

        public virtual bool CanShoot(float timeCounter)
        {
            if(timeCounter < Time.time || noCooldown)
            {
                return true;
            }else
            {
                return false;
            }
        }

        public virtual void Update()
        {
            GetInput();
        }

        private void Awake()
        {
            mainCam = MonoSingleton<CameraController>.Instance.transform;
            firePoint = transform.Find("viewModelWrapper/FirePoint");
            InitializeWeaponVariables();
        }

        public virtual void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && CanShoot(timeToFirePrimary))
            {
                timeToFirePrimary = fireDelayPrimary + Time.time;
                FirePrimary();
            }

            if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && CanShoot(timeToFireSecondary))
            {
                timeToFirePrimary = fireDelayPrimary + Time.time;
                FireSecondary();
            }
        }

        public virtual void InitializeWeaponVariables() { }

        public virtual void FirePrimary() {}

        public virtual void FireSecondary() {}
    }
}
