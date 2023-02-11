using HarmonyLib;
using Newtonsoft.Json.Linq;
using System.Collections;
using UMM;
using UnityEngine.Networking;

namespace UltraFunGuns
{
    [UKPlugin("Hydraxous.ULTRAKILL.UltraFunGuns", "UltraFunGuns", "1.1.8", "A mod that adds several goofy, wacky, and interesting weapons to ULTRAKILL", false, true)]
    public class UltraFunGuns : UKMod
    {
        public const string RELEASE_VERSION = "1.1.8-Experimental";
        const string GITHUB_URL = "https://api.github.com/repos/Hydraxous/ultrafunguns/tags";

        Harmony harmony = new Harmony("Hydraxous.ULTRAKILL.UltraFunGuns");

        public static bool UsingLatestVersion = true;
        public static string LatestVersion = "UNKNOWN";
        public static bool DebugMode => Data.Config.Data.DebugMode;

        public static UltraFunGuns UFG { get; private set; }

        private void Awake()
        {
            UFG = this;
        }

        public override void OnModLoaded()
        {
            HydraLogger.StartMessage();
            Data.CheckSetup();
            WeaponManager.Init();

            HydraLoader.LoadAssets((loaded) =>
            {
                if (loaded)
                {
                    CheckVersion();
                    DoPatching();
                    UKAPIP.Init();
                    Commands.Register();
                    HydraLogger.Log("Successfully Loaded!", DebugChannel.User);
                }
                else
                {
                    HydraLogger.Log("Loading failed.", DebugChannel.Fatal);
                    enabled = false;
                }
            });
        }

        public override void OnModUnload()
        {
            harmony.UnpatchSelf();
            Data.SaveAll();
            HydraLogger.WriteLog();
        }

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
                        HydraLogger.Log(string.Format("Error getting version info. Current Version: {0}", RELEASE_VERSION));
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