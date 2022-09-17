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

        private static AssetBundle assetBundle;

        public static bool dataRegistered = false;
        public static bool assetsRegistered = false;

        public static bool RegisterAll(byte[] bundle)
        {
            try
            {
                Debug.Log("HydraLoader: loading mod files");
                assetBundle = AssetBundle.LoadFromMemory(bundle);
                RegisterDataFiles();
                RegisterCustomAssets();
                Debug.Log("HydraLoader: loading complete");
                return true;
            }
            catch (Exception e)
            {
                Debug.Log("HydraLoader: loading failed");
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
                Debug.Log("HydraLoader: data registered successfully");
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
                    for (int i = 0; i < asset.modules.Length; i++)
                    {
                        newPrefab.AddComponent(asset.modules[i].GetType());
                    }
                    prefabRegistry.Add(asset.name, newPrefab);
                }
                Debug.Log("HydraLoader: prefabs registered successfully");

                assetsRegistered = true;
            }
        }

        public class CustomAssetPrefab
        {
            public Component[] modules;
            public string name;

            public CustomAssetPrefab(string assetName, Component[] componentsToAdd)
            {
                this.name = assetName;
                this.modules = componentsToAdd;
                assetsToRegister.Add(this);
            }
        }

        public class CustomAssetData
        {
            public string name;
            public DataFile dataFile;
            public bool hasData = false;
            public Type dataType;

            public CustomAssetData(string dataName, DataFile dataFile) //For loading custom script data
            {
                this.hasData = true;
                this.name = dataName;
                this.dataFile = dataFile;
                dataToRegister.Add(this);
            }

            public CustomAssetData(string dataName, Type type) //For loading general assets
            {
                this.name = dataName;
                this.dataType = type;

                dataToRegister.Add(this);
            }
        }
    }

    public class DataFile : UnityEngine.Object { }
}
