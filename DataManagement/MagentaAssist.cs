using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using GameConsole.Commands;

namespace UltraFunGuns.Datas
{
    public static class MagentaAssist
    {

        private static Magenta magenta = new(Path.Combine(Application.streamingAssetsPath, "Magenta"));

        public static bool LoadAsset<T>(string key, out UnityEngine.Object outputObject) where T : UnityEngine.Object
        {
            string lolLmao = AssetManager.Instance.gameObject.name;

            string text = MonoSingleton<AssetManager>.Instance.assetDependencies[key];
            MonoSingleton<AssetManager>.Instance.LoadBundles(new string[]
            {
                text
            });
            outputObject = MonoSingleton<AssetManager>.Instance.loadedBundles[text].LoadAsset<T>(key);

            return outputObject != null;
        }


        private static void CheckMagenta()
        {
            AssetManager.Instance.SendMessage("hello");
            HydraLogger.Log($"Magenta Set.");
        }

        public static void CheckBundles()
        {


            HydraLogger.Log("Checking bundles.");
            string currentBundleFile = File.ReadAllText(unhardenedBundlePath);

            //lol
            if (!currentBundleFile.Contains("Assets/Models/Developers/Hakita.png"))
            {
                PatchUnhardened();
            }

            CheckMagenta();
        }


        private static string unhardenedBundlePath = Path.Combine(Application.streamingAssetsPath, "Magenta", "unhardenedBundles.json");

        private static void PatchUnhardened()
        {
            File.WriteAllText(unhardenedBundlePath, Properties.Resources.UnhardendedBundlesJson);
        }
    }
}
