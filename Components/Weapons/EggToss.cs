using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class EggToss : UltraFunGunBase
    {
        private GameObject thrownEggPrefab;
        private Animator eggThrowAnimator;
        public float forceMultiplier = 2.0f;
        public override void InitializeWeaponVariables()
        {
            eggThrowAnimator = GetComponent<Animator>();
            HydraLoader.prefabRegistry.TryGetValue("ThrownEgg", out thrownEggPrefab);
            fireDelayPrimary = 0.6f;
            fireDelaySecondary = 1.2f;
        }

        public override void DoAnimations()
        {
            eggThrowAnimator.SetBool("CanShoot", CanShoot(timeToFirePrimary));
        }

        public override void FirePrimary()
        {
            eggThrowAnimator.SetTrigger("Shoot");
            GameObject newThrownEgg = GameObject.Instantiate<GameObject>(thrownEggPrefab,mainCam.TransformPoint(0,0,0.5f),Quaternion.identity);
            newThrownEgg.transform.forward = mainCam.forward;
            newThrownEgg.GetComponent<Rigidbody>().velocity = mainCam.TransformDirection(0, 0, 1 * forceMultiplier);

        }

        public override void FireSecondary()
        {
            
        }
    }
}
