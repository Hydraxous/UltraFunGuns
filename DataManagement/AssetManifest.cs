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
            //new HydraLoader.CustomAssetPrefab("Prototype_Weapon", new Component[] { new WeaponIcon(), new WeaponIdentifier() });
            //new HydraLoader.CustomAssetPrefab("WMUINode", new Component[] { new InventoryNode() });
            //new HydraLoader.CustomAssetPrefab("UFGInventoryUI", new Component[] { new InventoryController(), new HudOpenEffect() });
            //new HydraLoader.CustomAssetPrefab("UFGInventoryButton");
            #endregion

            #region Weapon Objects

            UFGWeapon[] weaponInfos = WeaponManager.GetWeapons();

            for(int i=0;i < weaponInfos.Length;i++)
            {
                new HydraLoader.CustomAssetPrefab(weaponInfos[i].WeaponKey, new Component[] { new WeaponIcon() { variationColor=(int) weaponInfos[i].IconColor }, new WeaponIdentifier() });
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
