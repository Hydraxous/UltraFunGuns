using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    
    [FunGun("MysticFlare", "Mystic Flaaaare", 0, true, WeaponIconColor.Red)]
    [WeaponAbility("Primary", "Fires a projectile attack! using <color=orange>Fire 1</color>.", 6, RichTextColors.red)]
    [WeaponAbility("Secondary", "Shits and farts usiong <color=orange>Fire 2</color>.", 2, RichTextColors.lime)]
    public class MysticFlare : UltraFunGunBase
    {
        public override void FirePrimary()
        {
            base.FirePrimary();
        }

        public override void FireSecondary()
        {
            base.FireSecondary();
        }

    }

}