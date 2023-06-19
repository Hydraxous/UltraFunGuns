using BepInEx;
using HarmonyLib;
using HydraDynamics;
using HydraDynamics.Debugging;
using System;
using System.Collections;
using UltraFunGuns.Datas;
using UltraFunGuns.Patches;
using UltraFunGuns.Util;

namespace UltraFunGuns
{
    [BepInDependency("Hydraxous.HydraDynamics", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ConstInfo.GUID, ConstInfo.NAME, ConstInfo.VERSION)]
    [HydynamicsInfo(ConstInfo.NAME, ConstInfo.GUID, ConstInfo.VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class UltraFunGuns : BaseUnityPlugin
    {
        Harmony harmony = new Harmony("Hydraxous.ULTRAKILL.UltraFunGuns");

        public static bool UsingLatestVersion = true;
        public static string LatestVersion = "UNKNOWN";

        private UltraFunGunBase.ActionCooldown autosave = new UltraFunGunBase.ActionCooldown(120f);

        private Action<bool, string> onVersionCheckFinished = (usingLatest, latestVersion) =>
        {
            UsingLatestVersion = usingLatest;
            LatestVersion = latestVersion;
        };

        public static UltraFunGuns UFG { get; private set; }

        private void Awake()
        {
            UFG = this;
            Data.CheckSetup();
            StartCoroutine(Startup());
        }

        private IEnumerator Startup()
        {
            //HydraLogger.StartMessage();
            WeaponManager.Init();

            HydraLoader.LoadAssets((loaded) =>
            {
                if (loaded)
                {
                    HydraLogger.StartMessage();
                    UltraLoader.LoadAll();
                    VersionCheck.CheckVersion(ConstInfo.GITHUB_URL, ConstInfo.RELEASE_VERSION, onVersionCheckFinished);
                    DoPatching();
                    InGameCheck.Init();
                    CustomPlacedObjects.CustomPlacedObjectManager.Init();
                    CheatsPatch.NoHighScorePatch.Init();
                    Commands.Register();
                    FreecamAssist.Init();
                    TextureLoader.Init();
                    CompatibilityCheck.DoCheck();
                    HydraLogger.Log("Successfully Loaded!", DebugChannel.User);
                    gameObject.AddComponent<DebuggingDummy>();
                }
                else
                {
                    HydraLogger.Log("Loading failed.", DebugChannel.Fatal);
                    enabled = false;
                }
            });

            yield return null;
        }

        private void Update()
        {
            if(autosave.CanFire() && Data.Config.Data.EnableAutosave)
            {
                autosave.AddCooldown();
                Data.SaveAll();
            }
        }

        private void DoPatching()
        {
            harmony.PatchAll();
        }

        [Commands.UFGDebugMethod("Toggle Debug", "Toggles the debug mode for UFG.")]
        public static void ToggleDebugMode()
        {
            bool debugMode = !DebugMode;
            Data.Config.Data.DebugMode = debugMode;
            Data.Config.Save();
         
            HydraLogger.Log($"Debug mode: {((debugMode) ? "Enabled" : "Disabled")}", DebugChannel.User);
        }

        private void OnApplicationQuit()
        {
            Data.SaveAll();
            HydraLogger.WriteLog();
        }

        private void OnDisable()
        {
            HydraLogger.WriteLog();
        }

        public static bool DebugMode
        {
            get
            {
                if (Data.Config != null)
                {
                    return Data.Config.Data.DebugMode;
                }

                return false;
            }
        }
    }

}