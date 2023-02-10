using UnityEngine;
using System;
using System.Collections;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UltraFunGuns.Properties;
using UMM;
using HarmonyLib;

namespace UltraFunGuns
{
    [UKPlugin("Hydraxous.ULTRAKILL.UltraFunGuns", "UltraFunGuns", "1.1.8", "A mod that adds several goofy, wacky, and interesting weapons to ULTRAKILL", false, false)]
    public class UltraFunGuns : UKMod
    {
        public static bool UsingLatestVersion = true;
        public static bool UsedWeapons = true;
        public static string Version = "1.1.8-Experimental";
        public static string LatestVersion = "UNKNOWN";
        public static bool DebugMode = true;
        private static string githubURL = "https://api.github.com/repos/Hydraxous/ultrafunguns/tags";

        private void Awake()
        {
            HydraLogger.Log($"UltraFunGuns loading started. Version: {Version}");

            Data.CheckSetup();
            WeaponManager.Init();

            if (AssetManifest.RegisterAssets())
            {
                CheckVersion();
                DoPatching();
                UKAPIP.Init();
                //UKAPIP.OnLevelChanged += (_) => CheckWeapons();
                Commands.Register();
                HydraLogger.Log("UltraFunGuns loaded.", DebugChannel.User);
            }
            else
            {
                HydraLogger.Log("UltraFunGuns failed to load.", DebugChannel.Fatal);
                this.enabled = false;
            }
        }

        private void DoPatching()
        {
            Harmony harmony = new Harmony("Hydraxous.ULTRAKILL.UltraFunGuns");
            harmony.PatchAll();
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
                        LatestVersion = JArray.Parse(page)[0].Value<string>("name");
                        UsingLatestVersion = (LatestVersion == Version);
                        if (UsingLatestVersion)
                        {
                            HydraLogger.Log(string.Format("You are using the latest version of UFG: {0}", LatestVersion), DebugChannel.User);
                        }
                        else
                        {
                            HydraLogger.Log(string.Format("New version of UFG available: {0}. Please consider updating.", LatestVersion), DebugChannel.User);
                        }
                    }
                    catch (System.Exception e)
                    {
                        UsingLatestVersion = true;
                        HydraLogger.Log(string.Format("Error getting version info. Current Version: {0}", Version));
                    }

                }
            }
        }

        private void OnApplicationQuit()
        {
            Data.SaveAll();
            HydraLogger.WriteLog();
        }
    }

}