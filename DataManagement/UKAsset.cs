using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine.InputSystem;

namespace UltraFunGuns.Datas
{
    public class UKAsset<T> where T : UnityEngine.Object
    {
        private T asset;
        public T Asset
        {
            get
            {
                if(asset == null)
                {
                    asset = LoadAsset<T>();
                }
                return asset;
            }
        }

        private string path;
        public string AssetName => Path.GetFileName(path);

        //Literally just does what AssetHelper.LoadPrefab does but for every type.
        private T LoadAsset<T>() where T : UnityEngine.Object
        {
            string lolLmao = AssetManager.Instance.gameObject.name; //Make sure the assetmanager gets instanced so we do not get NRE's

            if(!AssetManager.Instance.assetDependencies.ContainsKey(path))
            {
                HydraLogger.Log($"UKAsset: {path} does not exist.",DebugChannel.Fatal);
                return null;
            }

            string text = MonoSingleton<AssetManager>.Instance.assetDependencies[path];
            MonoSingleton<AssetManager>.Instance.LoadBundles(new string[]
            {
                text
            });
            UnityEngine.Object outputObject = MonoSingleton<AssetManager>.Instance.loadedBundles[text].LoadAsset<T>(AssetName);

            return (T) outputObject;
        }

        public UKAsset(string key) 
        {
            path= key;
        }
    }
}
