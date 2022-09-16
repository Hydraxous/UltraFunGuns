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

        public override void InitializeWeaponVariables()
        {
            eggThrowAnimator = GetComponent<Animator>();
            HydraLoader.prefabRegistry.TryGetValue("ThrownEgg", out thrownEggPrefab);
            fireDelayPrimary = 1.2f;
            fireDelaySecondary = 1.2f;
        }

        public override void FirePrimary()
        {
            
        }

        public override void FireSecondary()
        {
            
        }
    }
}
