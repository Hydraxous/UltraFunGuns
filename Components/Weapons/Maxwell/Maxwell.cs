using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns
{
    [WeaponAbility("Cat Power", "Explodes everything in existence it's kind of funny if you think about it.", 0, RichTextColors.fuchsia)]
    [WeaponAbility("Cat Vision", "Can see for fuckin miles", 1, RichTextColors.orange)]
    [FunGun("Maxwell","Maxwell", 3, true, WeaponIconColor.Yellow, false)]
    public class Maxwell : UltraFunGunBase
    {
        public override void FirePrimary()
        {
            HydraLogger.Log("Penis");
        }
    }
}
