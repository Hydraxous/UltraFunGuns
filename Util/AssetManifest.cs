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
        public static bool RegisterAssets()
        {
            if(assetsRegistered)
            {
                return false;
            }

            #region UI Stuff
            //Generic, debug, etc. assets
            new HydraLoader.CustomAssetData("debug_weaponIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("debug_glowIcon", typeof(Sprite));
            //new HydraLoader.CustomAssetPrefab("Prototype_Weapon", new Component[] { new WeaponIcon(), new WeaponIdentifier() });
            new HydraLoader.CustomAssetPrefab("WMUINode", new Component[] { new InventoryNode() });
            new HydraLoader.CustomAssetPrefab("UFGInventoryUI", new Component[] { new InventoryController(), new HudOpenEffect() });
            new HydraLoader.CustomAssetPrefab("UFGInventoryButton", new Component[] { });
            #endregion

            #region sonic gun
            //SonicReverberator
            new HydraLoader.CustomAssetPrefab("SonicReverberationExplosion", new Component[] { new SonicReverberatorExplosion() });
            new HydraLoader.CustomAssetPrefab("SonicReverberator", new Component[] { new SonicReverberator(), new WeaponIcon(), new WeaponIdentifier() });
            //Sonic gun gyros data
            new HydraLoader.CustomAssetData("InnerGyroBearing", new GyroRotator.GyroRotatorData(1.0f, Vector3.forward, 0.004f, 3f, 40.66f));
            new HydraLoader.CustomAssetData("MiddleGyro", new GyroRotator.GyroRotatorData(1.2f, new Vector3(1, 0, 0), 0.004f, 3.5f, -53.58f));
            new HydraLoader.CustomAssetData("InnerGyro", new GyroRotator.GyroRotatorData(1.5f, Vector3.back, 0.004f, 4f, -134.3f));
            new HydraLoader.CustomAssetData("Moyai", new GyroRotator.GyroRotatorData(1.5f, Vector3.one, 0.005f, 15f, -248.5f));
            //Sonic gun audiofiles
            new HydraLoader.CustomAssetData("vB_loud", typeof(AudioClip));
            new HydraLoader.CustomAssetData("vB_loudest", typeof(AudioClip));
            new HydraLoader.CustomAssetData("vB_standard", typeof(AudioClip));
            //Sonic gun icons
            new HydraLoader.CustomAssetData("SonicReverberator_glowIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("SonicReverberator_weaponIcon", typeof(Sprite));
            #endregion

            #region egg wep
            //Egg :)
            new HydraLoader.CustomAssetPrefab("EggToss", new Component[] { new EggToss(), new WeaponIcon(), new WeaponIdentifier() });
            new HydraLoader.CustomAssetPrefab("ThrownEgg", new Component[] { new ThrownEgg(), new DestroyAfterTime() });
            new HydraLoader.CustomAssetPrefab("EggImpactFX", new Component[] { new DestroyAfterTime() });
            new HydraLoader.CustomAssetPrefab("EggSplosion", new Component[] { new EggSplosion(), new DestroyAfterTime() });
            //Icons
            new HydraLoader.CustomAssetData("EggToss_weaponIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("EggToss_glowIcon", typeof(Sprite));
            #endregion

            #region Dodgeball
            //Dodgeball
            new HydraLoader.CustomAssetPrefab("Dodgeball", new Component[] { new Dodgeball(), new WeaponIcon(), new WeaponIdentifier() });
            new HydraLoader.CustomAssetPrefab("ThrownDodgeball", new Component[] { new ThrownDodgeball() });
            new HydraLoader.CustomAssetPrefab("DodgeballImpactSound", new Component[] { new DestroyAfterTime() });
            new HydraLoader.CustomAssetPrefab("DodgeballPopFX", new Component[] { new DestroyAfterTime() });
            new HydraLoader.CustomAssetData("BasketballMaterial", typeof(Material));
            //Icons 
            new HydraLoader.CustomAssetData("Dodgeball_weaponIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("Dodgeball_glowIcon", typeof(Sprite));
            #endregion

            #region Focalyzer
            #region Standard
            //Focalyzer
            new HydraLoader.CustomAssetPrefab("Focalyzer", new Component[] { new Focalyzer(), new WeaponIcon(), new WeaponIdentifier() });
            new HydraLoader.CustomAssetPrefab("FocalyzerPylon", new Component[] { new FocalyzerPylon() });
            new HydraLoader.CustomAssetPrefab("FocalyzerLaser", new Component[] { new FocalyzerLaserController() });
            //Icons
            new HydraLoader.CustomAssetData("Focalyzer_glowIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("Focalyzer_weaponIcon", typeof(Sprite));
            #endregion

            #region Focalyzer_Alternate
            //FocalyzerAlternate
            new HydraLoader.CustomAssetPrefab("FocalyzerAlternate", new Component[] { new FocalyzerAlternate(), new WeaponIcon(), new WeaponIdentifier() });
            new HydraLoader.CustomAssetPrefab("FocalyzerPylonAlternate", new Component[] { new FocalyzerPylonAlternate() });
            new HydraLoader.CustomAssetPrefab("FocalyzerLaserAlternate", new Component[] { new FocalyzerLaserControllerAlternate() });
            //Icons 
            new HydraLoader.CustomAssetData("FocalyzerAlternate_glowIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("FocalyzerAlternate_weaponIcon", typeof(Sprite));
            #endregion
            #endregion

            #region TrickSniper
            //Tricksniper
            new HydraLoader.CustomAssetPrefab("Tricksniper", new Component[] { new Tricksniper(), new WeaponIcon(), new WeaponIdentifier() });
            new HydraLoader.CustomAssetPrefab("BulletTrail", new Component[] { new DestroyAfterTime() });
            new HydraLoader.CustomAssetPrefab("BulletPierceTrail", new Component[] { new DestroyAfterTime() });
            new HydraLoader.CustomAssetPrefab("TricksniperMuzzleFX", new Component[] { new DestroyAfterTime() });
            #endregion

            #region bulletstorm
            //Bulletstorm
            new HydraLoader.CustomAssetPrefab("Bulletstorm", new Component[] { new Bulletstorm(), new WeaponIcon(), new WeaponIdentifier() });
            #endregion

            #region handgun
            //Fingerguns
            new HydraLoader.CustomAssetPrefab("FingerGun_ImpactExplosion", new Component[] { new DestroyAfterTime() });
            new HydraLoader.CustomAssetPrefab("FingerGun", new Component[] { new FingerGun(), new WeaponIcon(), new WeaponIdentifier() });
            //Icon
            new HydraLoader.CustomAssetData("FingerGun_weaponIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("FingerGun_glowIcon", typeof(Sprite));
            #endregion

            #region payloader
            //Grenade variant


            //CanLauncher variant
            new HydraLoader.CustomAssetPrefab("CanLauncher", new Component[] { new CanLauncher(), new WeaponIcon(), new WeaponIdentifier() });
            new HydraLoader.CustomAssetPrefab("CanLauncher_CanProjectile", new Component[] { new CanProjectile() });
            new HydraLoader.CustomAssetPrefab("CanLauncher_CanExplosion", new Component[] { new CanExplosion(), new DestroyAfterTime() });
            new HydraLoader.CustomAssetPrefab("CanLauncher_CanProjectile_BounceFX", new Component[] { new DestroyAfterTime(), new AlwaysLookAtCamera() { useXAxis = true, useYAxis = true, useZAxis = true } });
            new HydraLoader.CustomAssetPrefab("CanLauncher_MuzzleFX", new Component[] { new DestroyAfterTime() });

            //Can Materials
            for (int i = 0; i < 10; i++)
            {
                new HydraLoader.CustomAssetData(string.Format("CanLauncher_CanProjectile_Material_{0}", i), typeof(Material));
            }
            #endregion

            #region RemoteBomb

            new HydraLoader.CustomAssetPrefab("RemoteBomb", new Component[] { new RemoteBomb(), new WeaponIcon(), new WeaponIdentifier() });
            new HydraLoader.CustomAssetPrefab("RemoteBomb_Explosive", new Component[] { new RemoteBombExplosive() });
            new HydraLoader.CustomAssetPrefab("RemoteBomb_Explosive_Explosion", new Component[] { new DestroyAfterTime(), new AudioSourceRandomizer() });

            #endregion

            #region JetSpear
            new HydraLoader.CustomAssetPrefab("JetSpear", new Component[] { new JetSpear(), new WeaponIcon(), new WeaponIdentifier() });

            #endregion


            #region MysticFlare
            
            #endregion

            assetsRegistered = true;

            return HydraLoader.RegisterAll(Properties.UltraFunGunsResources.UltraFunGuns);
        }
    }
}
