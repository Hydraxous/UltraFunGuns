using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public static class HydraLoader
    {
        private static List<CustomAssetPrefab> assetsToRegister = new List<CustomAssetPrefab>();

        private static List<CustomAssetData> dataToRegister = new List<CustomAssetData>();

        public static Dictionary<string, UnityEngine.Object> dataRegistry = new Dictionary<string, UnityEngine.Object>();

        public static Dictionary<string, GameObject> prefabRegistry = new Dictionary<string, GameObject>();

        private static AssetBundleCreateRequest bundleRequest;

        public static void LoadAssets(Action<bool> onLoaderComplete)
        {
            UltraFunGuns.Log.Log("Loader: Loading Assetbundle.");

            //Check error here.
            bundleRequest = AssetBundle.LoadFromMemoryAsync(Properties.Resources.UltraFunGuns);

            //onLoader Callback NEEDS To be called if the mod fails to load.
            bundleRequest.completed += (async) =>
            {
                AssetManifest.RegisterAssets();
                AssetBundle = bundleRequest.assetBundle;
                UltraFunGuns.Log.Log("Loader: Assetbundle Loaded.");
                bool loadSuccess = RegisterAll();
                BundleLoaded = loadSuccess;
                onLoaderComplete?.Invoke(loadSuccess);

            };
        }

        public static void UnloadAssets()
        {
            if(!BundleLoaded)
            {
                return;
            }

            dataToRegister.Clear();
            assetsToRegister.Clear();

            prefabRegistry.Clear();
            dataRegistry.Clear();

            assetsRegistered = false;
            dataRegistered = false;

            if(AssetBundle != null)
            {
                AssetBundle.Unload(true);
                AssetBundle = null;
            }
        }

        public static bool BundleLoaded { get; private set; }

        public static AssetBundle AssetBundle { get; private set; }

        public static bool dataRegistered = false;
        public static bool assetsRegistered = false;

        public static T LoadAssetOfType<T>(string name) where T : UnityEngine.Object
        {
            if(!BundleLoaded)
            {
                return null;
            }

            return AssetBundle.LoadAsset<T>(name);
        }

        public static bool RegisterAll()
        {
            try
            {
                UltraFunGuns.Log.Log("Loader: Registering all assets.");
                RegisterDataFiles();
                RegisterCustomAssets();
                UltraFunGuns.Log.Log("Loader: Asset registration complete.");
                return true;
            }
            catch (System.Exception e)
            {
                UltraFunGuns.Log.LogError("Loader: Asset loading failed!");
                UltraFunGuns.Log.LogError($"Loader: {e.Message}");
                return false;
            }
        }

        public static bool RegisterAll(byte[] assetBundleObject)
        {
            try
            {
                UltraFunGuns.Log.Log("HydraLoader: loading mod assets");
                //assetBundle = AssetBundle.LoadFromMemory(assetBundleObject);
                RegisterDataFiles();
                RegisterCustomAssets();
                UltraFunGuns.Log.Log("HydraLoader: loading complete");
                return true;
            }
            catch (Exception e)
            {
                UltraFunGuns.Log.LogError($"HydraLoader: asset loading failed\n{e.Message}");
                return false;
            }
        }

        public static void RegisterDataFiles()
        {
            if (!dataRegistered)
            {
                foreach (CustomAssetData assetData in dataToRegister)
                {
                    if (assetData.hasData)
                    {
                        dataRegistry.Add(assetData.name, assetData.dataFile);
                    }
                    else
                    {
                        dataRegistry.Add(assetData.name, AssetBundle.LoadAsset(assetData.name, assetData.dataType));
                    }
                    
                }
                UltraFunGuns.Log.Log(String.Format("HydraLoader: {0} asset datas registered successfully", dataRegistry.Count));
                dataRegistered = true;
            }
        }

        public static void RegisterCustomAssets()
        {
            if (!assetsRegistered)
            {
                foreach (CustomAssetPrefab asset in assetsToRegister)
                {
                    GameObject newPrefab = AssetBundle.LoadAsset<GameObject>(asset.name);

                    if(newPrefab == null)
                    {
                        UltraFunGuns.Log.LogError($"HydraLoader: (Load Error): {asset.name} could not be found in assetbundle");
                        newPrefab = AssetBundle.LoadAsset<GameObject>("BrokenAsset");
                        newPrefab.name = asset.name;
                    }

                    for (int i = 0; i < asset.modules.Length; i++)
                    {
                        newPrefab.AddComponent(asset.modules[i].GetType());
                    }
                    if(!prefabRegistry.ContainsKey(asset.name))
                    {
                        prefabRegistry.Add(asset.name, newPrefab);
                    }
                }
                UltraFunGuns.Log.Log(String.Format("HydraLoader: {0} prefabs registered successfully", prefabRegistry.Count));

                assetsRegistered = true;
            }
        }

        public class CustomAssetPrefab
        {
            public Component[] modules;
            public string name;

            public CustomAssetPrefab(string assetName, Component[] componentsToAdd = null)
            {
                this.name = assetName;
                if(componentsToAdd == null)
                {
                    this.modules = new Component[] { };
                }else
                {
                    this.modules = componentsToAdd;
                }
                assetsToRegister.Add(this);
                UltraFunGuns.Log.Log(String.Format("prefab: {0}, registered successfully.", assetName));

            }
        }

        public class CustomAssetData
        {
            public string name;
            public DataFile dataFile;
            public bool hasData = false;
            public Type dataType;

            public CustomAssetData(string dataName, DataFile dataFile) //For loading custom script data, try not to use this.
            {
                this.hasData = true;
                this.name = dataName;
                this.dataFile = dataFile;
                dataToRegister.Add(this);
                UltraFunGuns.Log.Log(String.Format("{0} of type: {1} registered successfully.", dataName, dataFile.GetType().ToString()));
            }

            public CustomAssetData(string dataName, Type type) //For loading general assets
            {
                this.name = dataName;
                this.dataType = type;
                dataToRegister.Add(this);
                UltraFunGuns.Log.Log(String.Format("{0} of type: {1} registered successfully.", dataName, type.ToString()));
            }
        }
    }

    public class DataFile : UnityEngine.Object {}

    /*
    public class HydraLoaderObject : MonoBehaviour
    {
        private void Awake()
        {
            if(HydraLoader.BundleLoaded)
            {
                return;
            }

            StartCoroutine(LoadBundle());
        }

        private IEnumerator LoadBundle()
        {

        }

    }
    */
}
