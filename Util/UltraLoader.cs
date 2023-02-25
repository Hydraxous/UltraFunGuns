using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Mono.Cecil;
using UnityEngine;
using System.Net.Http.Headers;

namespace UltraFunGuns
{
    public static class UltraLoader
    {
        public static bool AssetsLoaded { get; private set; }

        public static bool LoadAll()
        {
            if (AssetsLoaded)
            {
                return false;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();

            HydraLogger.Log($"UltraLoader: Finding asset tags.");

            foreach (Type type in assembly.GetTypes())
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static);

                foreach (FieldInfo field in fields)
                {
                    if(!field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        continue;
                    }

                    if (!field.IsStatic)
                    {
                        continue;
                    }

                    var cacheOnLoad = field.GetCustomAttribute<UFGAsset>();
                    
                    if(cacheOnLoad == null)
                    {
                        continue;
                    }

                    HydraLogger.Log($"UltraLoader: Found asset tag {cacheOnLoad.Key}");

                    Type fieldType = field.FieldType;

                    UnityEngine.Object loadedAsset = HydraLoader.AssetBundle.LoadAsset(cacheOnLoad.Key, fieldType);

                    if (loadedAsset == null)
                    {
                        HydraLogger.Log($"{type.Name}:{field.Name}:CacheOnLoad: Attempted to load asset {cacheOnLoad.Key}, but it was not found in the assetbundle.", DebugChannel.Error);
                        continue;
                    }

                    HydraLogger.Log($"{cacheOnLoad.Key} cached to {field.Name}");

                    field.SetValue(null, loadedAsset);
                }
            }

            AssetsLoaded = true;
            HydraLogger.Log($"UltraLoader: Asset loading complete.");
            return true;
        }
    }
}
