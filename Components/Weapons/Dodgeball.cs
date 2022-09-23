using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    //Throwable projectile that bounces off of things, does damage to enemies and multiplies it's velocity every time it hits something. Should be really funny.
    public class Dodgeball : UltraFunGunBase
    {

        private bool throwingBall = false;

        private float ballStartingForce = 20.0f;
        private float impactForceMultiplier = 1.5f;
        private int maxBallImpacts = 20;

        public override void OnAwakeFinished()
        {
            base.OnAwakeFinished();
        }

        public override void FirePrimary()
        {
            base.GetInput();
        }

        public override void DoAnimations()
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, ActionCooldown> SetActionCooldowns()
        {

            return null;
        }



    }
}
