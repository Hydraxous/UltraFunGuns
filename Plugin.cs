using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using System;
using System.Collections;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UltraFunGuns.Properties;
using HarmonyLib;

namespace UltraFunGuns
{
    [BepInPlugin("Hydraxous.ULTRAKILL.UltraFunGuns", "UltraFunGuns", "1.1.8")]
    public class UltraFunGuns : BaseUnityPlugin
    {
        
        public UFGWeaponManager gunPatch;
        public InventoryControllerDeployer invControllerDeployer;

        public static bool usingLatestVersion = true;
        public static bool usedWeapons = true;
        public static string version = "1.1.8-Experimental";
        public static string latestVersion = "UNKNOWN";
        private static string githubURL = "https://api.github.com/repos/Hydraxous/ultrafunguns/tags";

        private void Awake()
        {
            if (RegisterAssets() && InventoryDataManager.Initialize())
            {
                CheckVersion();
                DoPatching();
                Logger.LogInfo("UltraFunGuns Loaded.");
            }else
            {
                this.enabled = false;
            }
        }

        private void CheckWeapons()
        {
            if (gunPatch != null && invControllerDeployer != null)
            {
                return;
            }

            if (invControllerDeployer == null)
            {
                CanvasController canvas = MonoSingleton<CanvasController>.Instance;
                if(!canvas.TryGetComponent<InventoryControllerDeployer>(out invControllerDeployer))
                {
                    usedWeapons = false;
                    invControllerDeployer = canvas.gameObject.AddComponent<InventoryControllerDeployer>();
                }

            }

            if(gunPatch == null)
            {
                GunControl gc = MonoSingleton<GunControl>.Instance;
                if (!gc.TryGetComponent<UFGWeaponManager>(out UFGWeaponManager ultraFGPatch))
                {
                    usedWeapons = false;
                    gunPatch = gc.gameObject.AddComponent<UFGWeaponManager>();
                    gunPatch.Slot7Key = SLOT_7_KEY.Value;
                    gunPatch.Slot8Key = SLOT_8_KEY.Value;
                    gunPatch.Slot9Key = SLOT_9_KEY.Value;
                    gunPatch.Slot10Key = SLOT_10_KEY.Value;
                }
            }
            
        }

        public static bool InLevel()
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

        //REGISTRY: Register custom assets for the loader here!
        private bool RegisterAssets()
        {
            BindConfigs();

            #region UI Stuff
            //Generic, debug, etc. assets
            new HydraLoader.CustomAssetData("debug_weaponIcon", typeof(Sprite));
            new HydraLoader.CustomAssetData("debug_glowIcon", typeof(Sprite));
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
            new HydraLoader.CustomAssetPrefab("Tricksniper", new Component[] { new Tricksniper(), new WeaponIcon(), new WeaponIdentifier()});
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
            new HydraLoader.CustomAssetPrefab("FingerGun", new Component[] { new FingerGun() , new WeaponIcon(), new WeaponIdentifier()});
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
            new HydraLoader.CustomAssetPrefab("CanLauncher_CanProjectile_BounceFX", new Component[] { new DestroyAfterTime(), new AlwaysLookAtCamera(){ useXAxis=true, useYAxis=true, useZAxis=true} });
            //Can Materials
            for(int i = 0;i<10;i++)
            {
                new HydraLoader.CustomAssetData(string.Format("CanLauncher_CanProjectile_Material_{0}",i), typeof(Material));
            }
            #endregion
            
            return HydraLoader.RegisterAll(UltraFunGunsResources.UltraFunGuns);          
        }

        //Turns on major assists if weapons are used.
        private static void UpdateMajorAssistUsage()
        {
            if (MonoSingleton<StatsManager>.Instance != null)
            {
                if (!MonoSingleton<StatsManager>.Instance.majorUsed)
                {
                    MonoSingleton<StatsManager>.Instance.majorUsed = usedWeapons;
                }
            }
        }

        private void Update()
        {
            try
            {
                if(!InLevel())
                {
                    usedWeapons = false;
                }
                CheckWeapons();
                UpdateMajorAssistUsage();
            }
            catch(System.Exception e)
            {

            }
            
        }

        public static ConfigEntry<bool> USE_BASKETBALL_TEXTURE;
        public static ConfigEntry<KeyCode> SLOT_7_KEY;
        public static ConfigEntry<KeyCode> SLOT_8_KEY;
        public static ConfigEntry<KeyCode> SLOT_9_KEY;
        public static ConfigEntry<KeyCode> SLOT_10_KEY;
        public static ConfigEntry<KeyCode> INVENTORY_KEY;


        private void BindConfigs()
        {
            USE_BASKETBALL_TEXTURE = Config.Bind("MISC", "USE_BASKETBALL_TEXTURE", false, "Setting to true will replace the dodgeball weapon texture to be a basketball. This was highly requested...");
            SLOT_7_KEY = Config.Bind("BINDINGS", "SLOT_7_KEY", KeyCode.Alpha7, "Keybind for a weapon slot, do not bind to existing binds in the vanilla game.");
            SLOT_8_KEY = Config.Bind("BINDINGS", "SLOT_8_KEY", KeyCode.Alpha8, "Keybind for a weapon slot, do not bind to existing binds in the vanilla game.");
            SLOT_9_KEY = Config.Bind("BINDINGS", "SLOT_9_KEY", KeyCode.Alpha9, "Keybind for a weapon slot, do not bind to existing binds in the vanilla game.");
            SLOT_10_KEY = Config.Bind("BINDINGS", "SLOT_10_KEY", KeyCode.Alpha0, "Keybind for a weapon slot, do not bind to existing binds in the vanilla game.");
            INVENTORY_KEY = Config.Bind("BINDINGS", "INVENTORY_KEY", KeyCode.I, "Keybind to open the inventory directly.");
        }

        public void SaveConfig()
        {
            Config.Save();
        }

        private void CheckVersion()
        {
            StartCoroutine(CheckLatestVersion());
        }

        //matches current mod version with latest release on github
        private IEnumerator CheckLatestVersion()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(githubURL))
            {
                yield return webRequest.SendWebRequest();

                if (!webRequest.isNetworkError)
                {
                    string page = webRequest.downloadHandler.text;
                    try
                    {
                        latestVersion = JArray.Parse(page)[0].Value<string>("name");
                        usingLatestVersion = (latestVersion == version);
                        if(usingLatestVersion)
                        {
                            Logger.LogInfo(string.Format("You are using the latest version of UFG: {0}", latestVersion));
                        }
                        else
                        {
                            Logger.LogInfo(string.Format("New version of UFG available: {0}. Please consider updating.", latestVersion));
                        }
                    }
                    catch (System.Exception e)
                    {
                        usingLatestVersion = true;
                        Logger.LogInfo(string.Format("Error getting version info. Current Version: {0}", version));
                    }
                    
                }
            }
        }
    }
    
}