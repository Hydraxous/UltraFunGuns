using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    
    [UFGWeapon("MysticFlare", "Mystic Flaaaare", 0, true, WeaponIconColor.Red)]
    [WeaponAbility("Primary", "Fires a projectile attack! using <color=orange>Fire 1</color>.", 6, RichTextColors.red)]
    [WeaponAbility("Secondary", "Shits and farts usiong <color=orange>Fire 2</color>.", 2, RichTextColors.lime)]
    public class MysticFlare : UltraFunGunBase
    {
        public GameObject flareProjectilePrefab;
        public float maxRange = 13.0f;

        private bool flareDeployed = false;

        private MysticFlareProjectile deployedFlare;

        public override void GetInput()
        {
            if(MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && !flareDeployed)
            {
                DeployFlare();
            }else if(flareDeployed)
            {
                ExplodeFlare();
            }
        }

        //Throw flare
        private void DeployFlare()
        {
            flareDeployed = true;
            //Deploy flare lol
        }

        private void ExplodeFlare()
        {
            if(deployedFlare == null)
            {
                return;
            }

            //Explode flare
        }

        //
        public void Pull()
        {

        }

    }

}