using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [WeaponAbility("Full-Auto 16x.50 BMG", "Fire a continuous stream of 16 .50 BMG cartridges simultaneously.", 0, RichTextColors.red)]
    [WeaponAbility("Full-Auto Explosives", "Fire a continuous stream of explosive bolts.", 1, RichTextColors.red)]
    [FunGun("AdminGun","Admin Gun", 0, true, WeaponIconColor.Red)]
    public class AdminGun : UltraFunGunBase
    {
        public override void FirePrimary()
        {
            HydraLogger.Log("Admin gun shooted", DebugChannel.Message);
            RaycastHit[] hits = Physics.RaycastAll(mainCam.position, mainCam.forward);
            foreach (RaycastHit hit in hits)
            {
                CheckHit(hit);
            }
        }

        private void CheckHit(RaycastHit hit)
        {
            if(hit.transform.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier eid))
            {
                eid.DeliverDamage(eid.gameObject, mainCam.forward * 10000, hit.point, 20000, true, 10000, gameObject);
            }

            if(hit.transform.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidid))
            {
                eidid.eid.DeliverDamage(eidid.eid.gameObject, mainCam.forward * 10000, hit.point, 20000, true, 10000, gameObject);

            }
        }
    }
}
