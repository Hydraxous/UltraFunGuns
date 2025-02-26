using BepInEx;
using Configgy;
using HarmonyLib;
using System;
using System.Collections;
using System.IO;
using UltraFunGuns.Logging;
using UltraFunGuns.Patches;
using UltraFunGuns.Util;
using UnityEngine;

namespace UltraFunGuns
{
    [BepInDependency("Hydraxous.ULTRAKILL.Configgy", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ConstInfo.GUID, ConstInfo.NAME, ConstInfo.VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class UltraFunGuns : BaseUnityPlugin
    {
        Harmony harmony = new Harmony(ConstInfo.GUID);
        ConfigBuilder config = new ConfigBuilder(ConstInfo.GUID, ConstInfo.NAME);

        public static bool UsingLatestVersion = true;
        public static string LatestVersion = "UNKNOWN";

        internal static Ilogger Log { get; private set; }

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
            Log = new BepInExLogger(Logger);
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
                    UltraFunGuns.Log.LogWarning(ConstInfo.FUN_HEADER);
                    Log.Log($"{ConstInfo.NAME} is loading... Version: {ConstInfo.RELEASE_VERSION}");
                    UltraLoader.LoadAll();
                    config.BuildAll();
                    Configgy.VersionCheck.CheckVersion(ConstInfo.GITHUB_VERSION_URL, ConstInfo.RELEASE_VERSION, onVersionCheckFinished);
                    DoPatching();
                    InGameCheck.Init();
                    CustomPlacedObjects.CustomPlacedObjectManager.Init();
                    NoHighScorePatch.Init();
                    Commands.Register();
                    TextureLoader.Init();
                    UltraFunGuns.Log.Log("Successfully Loaded!");
                    gameObject.AddComponent<DebuggingDummy>();
                }
                else
                {
                    UltraFunGuns.Log.LogError("Loading failed.");
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
        [Configgy.Configgable("Commands", displayName:"Toggle Debug Mode")]
        public static void ToggleDebugMode()
        {
            bool debugMode = !DebugMode;
            Data.Config.Data.DebugMode = debugMode;
            Data.Config.Save();
         
            UltraFunGuns.Log.Log($"Debug mode: {((debugMode) ? "Enabled" : "Disabled")}");
        }

        private void OnApplicationQuit()
        {
            Data.SaveAll();
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