using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    
    [WeaponInfo("MysticFlare", "Mystic Flaaaare", 0, true, WeaponIconColor.Red)]
    public class MysticFlare : UltraFunGunBase
    {

        [WeaponAbility("Primary","Fires a projectile attack! using <color=orange>Fire 1</color>.", 6, RichTextColors.red)]
        public override void FirePrimary()
        {
            base.FirePrimary();
        }

        [WeaponAbility("Secondary", "Shits and farts usiong <color=orange>Fire 2</color>.", 2, RichTextColors.lime)]
        public override void FireSecondary()
        {
            base.FireSecondary();
        }

    }

}