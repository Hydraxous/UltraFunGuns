using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;
using UltraFunGuns.Properties;
using HarmonyLib;

namespace UltraFunGuns
{
    [BepInPlugin("Hydraxous.ULTRAKILL.UltraFunGuns", "UltraFunGuns", "1.1.4")]
    public class UltraFunGuns : BaseUnityPlugin
    {
        UltraFunGunsPatch gunPatch;

        private void Awake()
        {
            if (RegisterAssets())
            {
                DoPatching();
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

        private void DoPatching()
        {
            Harmony harmony = new Harmony("Hydraxous.ULTRAKILL.UltraFunGuns.Patch");
            harmony.PatchAll();
        }

        private void BindConfigs()
        {

        }

        //REGISTRY: Register custom assets for the loader here! TODO IF ISSUES ARISE CHECK ORDER OF REGISTRATION.
        private bool RegisterAssets()
        {
            BindConfigs();

            //Generic, debug, etc. assets
            new HydraLoader.CustomAssetData("debug_weaponIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("debug_glowIcon", typeof(Sprite));
            

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


            //Egg :)
            new HydraLoader.CustomAssetPrefab("EggToss", new Component[] { new EggToss(), new WeaponIcon(), new WeaponIdentifier() });
            new HydraLoader.CustomAssetPrefab("ThrownEgg", new Component[] { new ThrownEgg(), new DestroyAfterTime() });
            new HydraLoader.CustomAssetPrefab("EggImpactFX", new Component[] { new DestroyAfterTime() });
            new HydraLoader.CustomAssetPrefab("EggSplosion", new Component[] { new EggSplosion(), new DestroyAfterTime() });
            //Icons
            new HydraLoader.CustomAssetData("EggToss_weaponIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("EggToss_glowIcon", typeof(Sprite));

            //Dodgeball
            new HydraLoader.CustomAssetPrefab("Dodgeball", new Component[] { new Dodgeball(), new WeaponIcon(), new WeaponIdentifier() });
            new HydraLoader.CustomAssetPrefab("ThrownDodgeball", new Component[] { new ThrownDodgeball() });
            new HydraLoader.CustomAssetPrefab("DodgeballImpactSound", new Component[] { new DestroyAfterTime() });
            new HydraLoader.CustomAssetPrefab("DodgeballPopFX", new Component[] { new DestroyAfterTime() });
            //TODO Icons 
            //new HydraLoader.CustomAssetData("EggToss_weaponIcon", typeof(Sprite));
            //new HydraLoader.CustomAssetData("EggToss_glowIcon", typeof(Sprite));


            //Focalyzer
            new HydraLoader.CustomAssetPrefab("Focalyzer", new Component[] { new Focalyzer(), new WeaponIcon(), new WeaponIdentifier() });
            new HydraLoader.CustomAssetPrefab("FocalyzerPylon", new Component[] { new FocalyzerPylon() }); 
            new HydraLoader.CustomAssetPrefab("FocalyzerLaser", new Component[] { new FocalyzerLaserController() });
            //Icons
            new HydraLoader.CustomAssetData("Focalyzer_glowIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("Focalyzer_weaponIcon", typeof(Sprite));


            return HydraLoader.RegisterAll(UltraFunGunsResources.UltraFunGuns);
            
        }

        private void TurnOnAssists()
        {
            if (MonoSingleton<StatsManager>.Instance != null)
            {
                if (!MonoSingleton<StatsManager>.Instance.majorUsed)
                {
                    MonoSingleton<StatsManager>.Instance.majorUsed = true;
                }
            }
        }

        private void Update()
        {
            try
            {
                CheckWeapons();
                TurnOnAssists();
            }
            catch(System.Exception e)
            {

            }
            
        }


    }
    
}