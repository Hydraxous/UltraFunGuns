using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace UltraFunGuns
{
    public static class HydraLoader
    {
        private static List<CustomAssetPrefab> assetsToRegister = new List<CustomAssetPrefab>();

        private static List<CustomAssetData> dataToRegister = new List<CustomAssetData>();

        public static Dictionary<string, UnityEngine.Object> dataRegistry = new Dictionary<string, UnityEngine.Object>();

        public static Dictionary<string, GameObject> prefabRegistry = new Dictionary<string, GameObject>();

        private static AssetBundle assetBundle;

        public static bool dataRegistered = false;
        public static bool assetsRegistered = false;

        public static bool RegisterAll(byte[] assetBundleObject)
        {
            try
            {
                HydraLogger.Log("HydraLoader: loading mod assets");
                assetBundle = AssetBundle.LoadFromMemory(assetBundleObject);
                RegisterDataFiles();
                RegisterCustomAssets();
                HydraLogger.Log("HydraLoader: loading complete");
                return true;
            }
            catch (Exception e)
            {
                HydraLogger.Log($"HydraLoader: asset loading failed\n{e.Message}", DebugChannel.Fatal);
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
                        dataRegistry.Add(assetData.name, assetBundle.LoadAsset(assetData.name, assetData.dataType));
                    }
                    
                }
                HydraLogger.Log(String.Format("HydraLoader: {0} asset datas registered successfully", dataRegistry.Count));
                dataRegistered = true;
            }
        }

        public static void RegisterCustomAssets()
        {
            if (!assetsRegistered)
            {
                foreach (CustomAssetPrefab asset in assetsToRegister)
                {
                    GameObject newPrefab = assetBundle.LoadAsset<GameObject>(asset.name);

                    if(newPrefab == null)
                    {
                        HydraLogger.Log($"HydraLoader: (Load Error): {asset.name} could not be found in assetbundle: {assetBundle.name}", DebugChannel.Error);
                        newPrefab = assetBundle.LoadAsset<GameObject>("BrokenAsset");
                        newPrefab.name = asset.name;
                        newPrefab.AddComponent<HLErrorNotifier>();
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
                HydraLogger.Log(String.Format("HydraLoader: {0} prefabs registered successfully", prefabRegistry.Count));

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
                HydraLogger.Log(String.Format("prefab: {0}, registered successfully.", assetName));

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
                HydraLogger.Log(String.Format("{0} of type: {1} registered successfully.", dataName, dataFile.GetType().ToString()));
            }

            public CustomAssetData(string dataName, Type type) //For loading general assets
            {
                this.name = dataName;
                this.dataType = type;
                dataToRegister.Add(this);
                HydraLogger.Log(String.Format("{0} of type: {1} registered successfully.", dataName, type.ToString()));
            }
        }
    }

    public class DataFile : UnityEngine.Object {}

}
