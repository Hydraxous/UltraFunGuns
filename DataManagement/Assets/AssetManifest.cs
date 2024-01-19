using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    //Declaration of assets for use by components
    public static class AssetManifest
    {
        private static bool assetsRegistered = false;

        //REGISTRY: Register custom assets for the loader here!
        //TODO Get rid of this class
        //I cant get rid of this class simply due to the fact that I cannot cast arrays derived from UnityEngine.Object as a UnityEngine.Obejct[]. This would have been how the new loader worked.
        //However, it doesnt work because Unity :)))) So a work around will be needed at a later date.
        public static void RegisterAssets()
        {
            if(assetsRegistered)
            {
                return;
            }

            #region UI Stuff
            //Generic, debug, etc. assets
            new HydraLoader.CustomAssetData("debug_weaponIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("debug_glowIcon", typeof(Sprite));
            #endregion

            #region Weapon Objects

            UFGWeapon[] weaponInfos = WeaponManager.GetWeapons();

            for(int i=0;i < weaponInfos.Length;i++)
            {
                new HydraLoader.CustomAssetPrefab(weaponInfos[i].WeaponKey);
                new HydraLoader.CustomAssetData($"{weaponInfos[i].WeaponKey}_glowIcon", typeof(Sprite));
                new HydraLoader.CustomAssetData($"{weaponInfos[i].WeaponKey}_weaponIcon", typeof(Sprite));
            }

            #endregion

            #region payloader
            //Grenade variant
         
            //Can Materials
            for (int i = 0; i < 10; i++)
            {
                new HydraLoader.CustomAssetData(string.Format("CanLauncher_CanProjectile_Material_{0}", i), typeof(Material));
            }
            #endregion
            assetsRegistered = true;
        }
    }
}
