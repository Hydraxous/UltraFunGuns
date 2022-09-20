using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class EggToss : MonoBehaviour
    {
        private GameObject thrownEggPrefab;
        private Animator eggThrowAnimator;
        public float forceMultiplier = 50.0f;
        public bool noCooldown = false;
        private float fireDelayPrimary = 0.6f;
        private float timeToFirePrimary = 0.0f;

        private Transform mainCam, firePoint;

        private NewMovement player;

        private void Update()
        {
            GetInput();
            DoAnimations();
        }

        private void Start()
        {
            firePoint = transform.Find("viewModelWrapper/firePoint");
            mainCam = MonoSingleton<CameraController>.Instance.transform;
            player = MonoSingleton<NewMovement>.Instance;
            WeaponIcon wepIcon = gameObject.GetComponent<WeaponIcon>();
            wepIcon.variationColor = 1;
            eggThrowAnimator = GetComponent<Animator>();

            //TODO FIX LOAD ISSUES I think its the casting that does it .


            HydraLoader.dataRegistry.TryGetValue("EggToss_weaponIcon", out UnityEngine.Object EggToss_weaponIcon);
            wepIcon.weaponIcon = (Sprite) EggToss_weaponIcon;

            HydraLoader.dataRegistry.TryGetValue("EggToss_glowIcon", out UnityEngine.Object EggToss_glowIcon);
            wepIcon.glowIcon = (Sprite) EggToss_glowIcon;


            HydraLoader.prefabRegistry.TryGetValue("ThrownEgg", out thrownEggPrefab);
            HydraLoader.prefabRegistry.TryGetValue("EggImpactFX", out thrownEggPrefab.GetComponent<ThrownEgg>().impactFX);
            //fireDelayPrimary = 0.6f;
            //fireDelaySecondary = 1.2f;

        }

        private void DoAnimations()
        {
            eggThrowAnimator.SetBool("CanShoot", CanShoot(timeToFirePrimary));
        }

        private void FirePrimary()
        {
            eggThrowAnimator.Play("EggTossThrow");
            GameObject newThrownEgg = GameObject.Instantiate<GameObject>(thrownEggPrefab,firePoint.position,Quaternion.identity);
            newThrownEgg.transform.forward = mainCam.forward;
            Vector3 newVelocity = mainCam.TransformDirection(0, 0, 1 * forceMultiplier);
            newVelocity += player.rb.velocity;
            newThrownEgg.GetComponent<Rigidbody>().velocity = newVelocity;

        }

        private void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame && CanShoot(timeToFirePrimary))
            {
                timeToFirePrimary = fireDelayPrimary + Time.time;
                FirePrimary();
            }
        }

        private bool CanShoot(float timeCounter)
        {
            if (timeCounter < Time.time || noCooldown)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
