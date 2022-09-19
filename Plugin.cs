using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;
using UltraFunGuns.Properties;

namespace UltraFunGuns
{
    [BepInPlugin("Hydraxous.ULTRAKILL.UltraFunGuns", "UltraFunGuns", "1.0.2")]
    public class UltraFunGuns : BaseUnityPlugin
    {
        UltraFunGunsPatch gunPatch;

        private void Awake()
        {
            if (RegisterAssets())
            {
                Logger.LogInfo("UltraFunGuns Loaded.");
            }else
            {
                this.enabled = false;
            }
        }

        private void CheckWeapons()
        {
            if (gunPatch != null)
            {
                return;
            }

            GunControl gc = MonoSingleton<GunControl>.Instance;
            if (!gc.TryGetComponent<UltraFunGunsPatch>(out UltraFunGunsPatch ultraFGPatch))
            {
                gunPatch = gc.gameObject.AddComponent<UltraFunGunsPatch>();
            }
        }

        private bool InLevel()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Intro" || sceneName == "Main Menu")
            {
                return false;
            }
            return true;
        }

        private void BindConfigs()
        {

        }

        private bool RegisterAssets()
        {
            BindConfigs();

            //Generic assets
            new HydraLoader.CustomAssetData("vB_loud", typeof(AudioClip));
            new HydraLoader.CustomAssetData("vB_loudest", typeof(AudioClip));
            new HydraLoader.CustomAssetData("vB_standard", typeof(AudioClip));
            new HydraLoader.CustomAssetData("SonicReverberator_glowIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("SonicReverberator_weaponIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("EggToss_weaponIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("EggToss_glowIcon", typeof(Sprite));

            //Sonic gun gyros
            new HydraLoader.CustomAssetData("InnerGyroBearing", new GyroRotator.GyroRotatorData(1.0f, Vector3.forward, 0.004f, 3f, 40.66f));
            new HydraLoader.CustomAssetData("MiddleGyro", new GyroRotator.GyroRotatorData(1.2f, new Vector3(1, 0, 0), 0.004f, 3.5f, -53.58f));
            new HydraLoader.CustomAssetData("InnerGyro", new GyroRotator.GyroRotatorData(1.5f, Vector3.back, 0.004f, 4f, -134.3f));
            new HydraLoader.CustomAssetData("Moyai", new GyroRotator.GyroRotatorData(1.5f, Vector3.one, 0.005f, 15f, -248.5f));

            //SonicReverberator
            new HydraLoader.CustomAssetPrefab("SonicReverberationExplosion", new Component[] {new SonicReverberatorExplosion()});
            new HydraLoader.CustomAssetPrefab("SonicReverberator", new Component[] {new SonicReverberator(), new WeaponIcon() });

            //Egg :)
            new HydraLoader.CustomAssetPrefab("EggToss", new Component[] { new EggToss(), new WeaponIcon() });
            new HydraLoader.CustomAssetPrefab("ThrownEgg", new Component[] { new ThrownEgg() });
            new HydraLoader.CustomAssetPrefab("EggImpactFX", new Component[] { new DestroyAfterTime() });

            return HydraLoader.RegisterAll(UltraFunGunsResources.UltraFunGuns);
            
        }

        private void Update()
        {
            try
            {
                CheckWeapons();
            }
            catch(System.Exception e)
            {

            }
            
        }
    }
}