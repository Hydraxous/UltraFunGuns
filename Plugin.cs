using BepInEx;
using HarmonyLib;
using HydraDynamics.Debugging;
using Newtonsoft.Json.Linq;
using System.Collections;
using UltraFunGuns.Datas;
using UnityEngine;
using UnityEngine.Networking;

namespace UltraFunGuns
{
    [BepInDependency("Hydraxous.HydraDynamics", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Hydraxous.ULTRAKILL.UltraFunGuns", "UltraFunGuns", "1.2.0")]
    public class UltraFunGuns : BaseUnityPlugin
    {
        public const string RELEASE_VERSION = "1.2.0-Experimental";
        const string GITHUB_URL = "https://api.github.com/repos/Hydraxous/ultrafunguns/tags";

        Harmony harmony = new Harmony("Hydraxous.ULTRAKILL.UltraFunGuns");

        public static bool UsingLatestVersion = true;
        public static string LatestVersion = "UNKNOWN";

        public static UltraFunGuns UFG { get; private set; }

        private void Awake()
        {
            UFG = this;
            Data.CheckSetup();
            HLogger.AddContextLogAction(() => { return $"Weapons Deployed: {WeaponManager.DeployedWeapons}"; });
            HLogger.Log(textHeader, HydraDynamics.Debugging.DebugChannel.Warning);
            HLogger.Log("UFG starting.");
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
                    UltraLoader.LoadAll();
                    CheckVersion();
                    DoPatching();
                    UKAPIP.Init();
                    Patches.CheatsPatch.CyberGrindPreventer.Init();
                    Commands.Register();
                    Deboog.Log("Successfully Loaded!", DebugChannel.User);
                    gameObject.AddComponent<DebuggingDummy>();
                }
                else
                {
                    Deboog.Log("Loading failed.", DebugChannel.Fatal);
                    enabled = false;
                }
            });

            yield return null;
        }

        /*
        public override void OnModLoaded()
        {
            UKPrefabs.LoadAll();
            HydraLogger.StartMessage();
            Data.CheckSetup();
            WeaponManager.Init();

            HydraLoader.LoadAssets((loaded) =>
            {
                if (loaded)
                {
                    UltraLoader.LoadAll();
                    CheckVersion();
                    DoPatching();
                    UKAPIP.Init();
                    Commands.Register();
                    UltraFunGuns.UFG.Logger.Log("Successfully Loaded!", DebugChannel.User);
                }
                else
                {
                    UltraFunGuns.UFG.Logger.Log("Loading failed.", DebugChannel.Fatal);
                    enabled = false;
                }
            });
        }

        public override void OnModUnload()
        {
            harmony.UnpatchSelf();
            Data.SaveAll();
        }
        */

        private void DoPatching()
        {
            harmony.PatchAll();
        }

        private void CheckVersion()
        {
            StartCoroutine(CheckLatestVersion());
        }

        //matches current mod version with latest release on github
        private IEnumerator CheckLatestVersion()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(GITHUB_URL))
            {
                yield return webRequest.SendWebRequest();

                if (!webRequest.isNetworkError)
                {
                    string page = webRequest.downloadHandler.text;
                    try
                    {
                        LatestVersion = JArray.Parse(page)[0].Value<string>("name");
                        UsingLatestVersion = (LatestVersion == RELEASE_VERSION);
                        if (UsingLatestVersion)
                        {
                            Deboog.Log(string.Format("You are using the latest version of UFG: {0}", LatestVersion), DebugChannel.User);
                        }
                        else
                        {
                            Deboog.Log(string.Format("New version of UFG available: {0}. Please consider updating.", LatestVersion), DebugChannel.User);
                        }
                    }
                    catch (System.Exception e)
                    {
                        UsingLatestVersion = true;
                        Deboog.Log(string.Format("Error getting version info. Current Version: {0}\n{1}\n{2}", RELEASE_VERSION, e.Message, e.StackTrace), DebugChannel.Error);
                    }

                }
            }
        }

        [Commands.UFGDebugMethod("Toggle Debug", "Toggles the debug mode for UFG.")]
        public static void ToggleDebugMode()
        {
            bool debugMode = !DebugMode;
            Data.Config.Data.DebugMode = debugMode;
            Data.Config.Save();
         
            Deboog.Log($"Debug mode: {((debugMode) ? "Enabled" : "Disabled")}", DebugChannel.User);
        }

        private void OnApplicationQuit()
        {
            Data.SaveAll();
            UltraFunGuns.UFG.HLogger.WriteLog();
        }

        private void OnDisable()
        {
            UltraFunGuns.UFG.HLogger.WriteLog();
        }

        const string textHeader = @"
   __  ______             ______            ______                
  / / / / / /__________ _/ ____/_  ______  / ____/_  ______  _____
 / / / / / __/ ___/ __ `/ /_  / / / / __ \/ / __/ / / / __ \/ ___/
/ /_/ / / /_/ /  / /_/ / __/ / /_/ / / / / /_/ / /_/ / / / (__  ) 
\____/_/\__/_/   \__,_/_/    \__,_/_/ /_/\____/\__,_/_/ /_/____/

Created By Hydraxous";


        private HLogger logger;

        public HLogger HLogger
        {
            get
            {
                if (logger == null)
                {
                    logger = new HLogger(this, Data.DataManager);
                }
                return logger;
            }

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