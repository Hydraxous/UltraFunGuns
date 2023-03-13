using System;
using System.Reflection;
using UnityEngine;

namespace UltraFunGuns
{

    //HydraLoader 2.0 This will automatically load tagged fields/properties with assets from either the game's assetbundle or a specified one. TODO implement variable bundles
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

            HydraLogger.Log($"AssetLoader: Finding asset tags.");

            foreach (Type type in assembly.GetTypes())
            {
                CheckType(type);
            }

            AssetsLoaded = true;
            HydraLogger.Log($"UltraLoader: Asset loading complete.");
            return true;
        }

        private static void CheckType(Type type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                ProcessMember(field);
            }

            PropertyInfo[] properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

            foreach (PropertyInfo property in properties)
            {
                ProcessMember(property);
            }
        }

        private static void ProcessMember(FieldInfo field)
        {
            if (!field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return;
            }

            if (!field.IsStatic)
            {
                return;
            }

            UFGAsset ufgAsset = field.GetCustomAttribute<UFGAsset>();

            if (ufgAsset == null)
            {
                return;
            }

            //Check if a key was provided to the attribute, otherwise use the member's name.
            string assetKey = (ufgAsset.Key != "") ? ufgAsset.Key : field.Name;

            HydraLogger.Log($"AssetLoader: Found asset tag {assetKey}");

            Type fieldType = field.FieldType;

            //AssetBundle bundle = (ufgAsset.UKPrefab) ? UKPrefabs.GameAssets : HydraLoader.AssetBundle;

            if(ufgAsset.UKPrefab)
            {
                object loadedAsset = UMM.UKAPI.LoadCommonAsset(assetKey);

                if(loadedAsset != null)
                {
                    HydraLogger.Log($"AssetLoader: {assetKey} ({field.FieldType}), successfully cached to {field.DeclaringType}.{field.Name}");

                    field.SetValue(null, loadedAsset);
                }else
                {
                    HydraLogger.Log($"Failed to find asset: {assetKey}");

                }
            }
            else
            {
                if (TryLoadAsset(assetKey, HydraLoader.AssetBundle, fieldType, out UnityEngine.Object loadedAsset))
                {
                    HydraLogger.Log($"AssetLoader: {assetKey} ({field.FieldType}), successfully cached to {field.DeclaringType}.{field.Name}");

                    field.SetValue(null, loadedAsset);
                }
                else
                {
                    HydraLogger.Log($"{field.DeclaringType.Name}:{field.Name}:CacheOnLoad: ({assetKey}) Load error, see above.", DebugChannel.Error);
                }
            }   
        }

        private static void ProcessMember(PropertyInfo property)
        {
            if (!property.PropertyType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return;
            }

            MethodInfo[] accessors = property.GetAccessors();

            foreach (MethodInfo accessor in accessors)
            {
                if(!accessor.IsStatic)
                {
                    return;
                }
            }

            UFGAsset ufgAsset = property.GetCustomAttribute<UFGAsset>();

            if (ufgAsset == null)
            {
                return;
            }

            //Check if a key was provided to the attribute, otherwise use the member's name.
            string assetKey = (ufgAsset.Key != "") ? ufgAsset.Key : property.Name;

            HydraLogger.Log($"AssetLoader: Found asset tag {assetKey}");

            if (!property.CanWrite)
            {
                HydraLogger.Log($"AssetLoader: {property.DeclaringType.Name}: {property.Name}: No setter found, unable to assign asset.", DebugChannel.Fatal);
                return;
            }

            Type propertyType = property.PropertyType;

            //AssetBundle bundle = (ufgAsset.UKPrefab) ? UKPrefabs.GameAssets : HydraLoader.AssetBundle;

            if (ufgAsset.UKPrefab)
            {
                object loadedAsset = UMM.UKAPI.LoadCommonAsset(assetKey);

                if (loadedAsset != null)
                {
                    HydraLogger.Log($"AssetLoader: {assetKey} ({propertyType.Name}), successfully cached to {propertyType.DeclaringType}.{property.Name}");

                    property.SetValue(null, loadedAsset);
                }
                else
                {
                    HydraLogger.Log($"Failed to find asset: {assetKey}");
                }
            }
            else
            {
                if (TryLoadAsset(assetKey, HydraLoader.AssetBundle, propertyType, out UnityEngine.Object loadedAsset))
                {
                    HydraLogger.Log($"AssetLoader: {assetKey} ({propertyType.Name}), successfully cached to {property.DeclaringType.Name}.{property.Name}");
                    property.SetValue(null, loadedAsset);
                }
                else
                {
                    HydraLogger.Log($"{property.DeclaringType.Name}:{property.Name}:CacheOnLoad: ({assetKey}) Load error, see above.", DebugChannel.Error);
                }
            }     
        }

        /// <summary>
        /// Attempts to load an asset from the assetbundle.
        /// </summary>
        /// <param name="key">Name of the asset</param>
        /// <param name="type">Type of the asset</param>
        /// <param name="obj">Returned asset</param>
        /// <returns></returns>
        private static bool TryLoadAsset(string key, AssetBundle bundle, Type type,out UnityEngine.Object obj)
        {
            obj = bundle.LoadAsset(key, type);

            if (obj == null)
            {
                HydraLogger.Log($"AssetLoader: Attempted to load asset {key} of type {type.Name}, but it was not found in the assetbundle.", DebugChannel.Error);
                return false;
            }

            return true;
        }
    }
}
