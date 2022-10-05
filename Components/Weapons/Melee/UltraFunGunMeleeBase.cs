using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;

namespace UltraFunGuns
{
    public class UltraFunGunMeleeBase : UltraFunGunBase
    {

        public float maxHitAngle;
        public float maxRange;

        public float damageMultiplier;
        public float forceMultiplier;

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {
            return base.SetActionCooldowns();
        }

        public override void GetInput()
        {
            
        }

        public virtual void DoMeleeAttack()
        {

        }

        public override void DoAnimations()
        {

        }
    }
}
