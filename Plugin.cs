using BepInEx;
using HarmonyLib;
using HydraDynamics;
using HydraDynamics.Debugging;
using System;
using System.Collections;
using UltraFunGuns.Datas;
using UltraFunGuns.Patches;

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

        private Action<bool, string> onVersionCheckFinished = (usingLatest, latestVersion) =>
        {
            UsingLatestVersion = usingLatest;
            LatestVersion = latestVersion;
        };

        public static UltraFunGuns UFG { get; private set; }

        private void Awake()
        {
            Freecam.SpawnFreecam();
            UFG = this;
            Data.CheckSetup();
            StartCoroutine(Startup());
        }

        private IEnumerator Startup()
        {
            //HydraLogger.StartMessage();
            MagentaAssist.CheckBundles();
            WeaponManager.Init();

            HydraLoader.LoadAssets((loaded) =>
            {
                if (loaded)
                {
                    HydraLogger.StartMessage();
                    UltraLoader.LoadAll();
                    VersionCheck.CheckVersion(ConstInfo.GITHUB_URL, ConstInfo.RELEASE_VERSION, onVersionCheckFinished);
                    DoPatching();
                    UKAPIP.Init();
                    CustomPlacedObjects.CustomPlacedObjectManager.Init();
                    CheatsPatch.CyberGrindPreventer.Init();
                    Commands.Register();
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