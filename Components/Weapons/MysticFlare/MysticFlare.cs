using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    
    [UFGWeapon("MysticFlare", "Mystic Flaaaare", 0, true, WeaponIconColor.Red, false)]
    [WeaponAbility("Primary", "Fires a projectile attack! using <color=orange>Fire 1</color>.", 6, RichTextColors.red)]
    [WeaponAbility("Secondary", "Shits and farts usiong <color=orange>Fire 2</color>.", 2, RichTextColors.lime)]
    public class MysticFlare : UltraFunGunBase
    {
        [UFGAsset("MysticFlareProjectile")] private static GameObject flareProjectilePrefab;
        public float maxRange = 13.0f;

        private MysticFlareProjectile deployedFlare;

        public override void GetInput()
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed)
            {
                if(deployedFlare ==null)
                    DeployFlare();

            }else if (deployedFlare != null)
            {
                ExplodeFlare();
            }
        }

        //Throw flare
        private void DeployFlare()
        {
            GameObject newFlare = Instantiate(flareProjectilePrefab, firePoint.position, Quaternion.identity);
            newFlare.transform.forward = mainCam.forward;
            deployedFlare = newFlare.GetComponent<MysticFlareProjectile>();

        }

        private void ExplodeFlare()
        {
            if(deployedFlare == null)
            {
                return;
            }

            deployedFlare.Detonate();
            deployedFlare = null;
        }
    }

}