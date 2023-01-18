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
        public UFGWeaponManager gunPatch;
        public InventoryControllerDeployer invControllerDeployer;

        public static bool UsingLatestVersion = true;
        public static bool UsedWeapons = true;
        public static string Version = "1.1.8-Experimental";
        public static string LatestVersion = "UNKNOWN";
        public static bool DebugMode = true;
        private static string githubURL = "https://api.github.com/repos/Hydraxous/ultrafunguns/tags";

        private void Awake()
        {
            if (RegisterAssets() && InventoryDataManager.Initialize())
            {
                CheckVersion();
                DoPatching();
                UKAPIP.Init();
                Debug.Log("UltraFunGuns Loaded.");
            }
            else
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
                if (!canvas.TryGetComponent<InventoryControllerDeployer>(out invControllerDeployer))
                {
                    UsedWeapons = false;
                    invControllerDeployer = canvas.gameObject.AddComponent<InventoryControllerDeployer>();
                }

            }

            if (gunPatch == null)
            {
                GunControl gc = MonoSingleton<GunControl>.Instance;
                if (!gc.TryGetComponent<UFGWeaponManager>(out UFGWeaponManager ultraFGPatch))
                {
                    UsedWeapons = false;
                    gunPatch = gc.gameObject.AddComponent<UFGWeaponManager>();
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
            return AssetManifest.RegisterAssets();
        }

        private void Update()
        {
            try
            {
                if (!UKAPIP.InLevel())
                {
                    UsedWeapons = false;
                }
                CheckWeapons();
            }
            catch (System.Exception e)
            {
                //Not sure why but there is a huge string of errors when the game is starting. So this is blank for that reason.
            }

        }

        private void BindConfigs()
        {
            if (PersistentModDataExists("USE_BASKETBALL_TEXTURE"))
                Dodgeball.USE_BASKETBALL_TEXTURE = RetrieveBooleanPersistentModData("USE_BASKETBALL_TEXTURE");
        }

        public void SaveConfig()
        {
            SetPersistentModData("USE_BASKETBALL_TEXTURE", Dodgeball.USE_BASKETBALL_TEXTURE.ToString());
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
                            Debug.Log(string.Format("You are using the latest version of UFG: {0}", LatestVersion));
                        }
                        else
                        {
                            Debug.Log(string.Format("New version of UFG available: {0}. Please consider updating.", LatestVersion));
                        }
                    }
                    catch (System.Exception e)
                    {
                        UsingLatestVersion = true;
                        Debug.Log(string.Format("Error getting version info. Current Version: {0}", Version));
                    }

                }
            }
        }
    }

}