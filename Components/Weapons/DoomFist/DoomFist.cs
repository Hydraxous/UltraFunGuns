using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    [WeaponAbility("Punch","Do a punch", 0, RichTextColors.aqua)]
    [UFGWeapon("DoomFist", "Hydraulic Gauntlet", 3, true, WeaponIconColor.Red)]
    public class DoomFist : UltraFunGunBase
    {
        public float chargeTime = 0.0f;
        public float maxChargeTime = 0.0f;

        public float maxTravelDistance = 24.0f;



        public override void OnAwakeFinished()
        {

        }

    }
}
