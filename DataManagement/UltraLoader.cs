using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using UltraFunGuns.Datas;
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
                if (field.FieldType.IsArray)
                {
                    if (!field.FieldType.GetElementType().IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            if (!field.IsStatic)
            {
                return;
            }

            UFGAsset assetTag = field.GetCustomAttribute<UFGAsset>();

            if (assetTag != null)
            {
                PopulateField(assetTag, field);
            }
        }

        private static void ProcessMember(PropertyInfo property)
        {
            if (!property.PropertyType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                if(property.PropertyType.IsArray)
                {
                    if(!property.PropertyType.GetElementType().IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        return;
                    }
                }else
                {
                    return;
                }
            }

            MethodInfo[] accessors = property.GetAccessors();

            foreach (MethodInfo accessor in accessors)
            {
                if (!accessor.IsStatic)
                {
                    return;
                }
            }

            UFGAsset assetTag = property.GetCustomAttribute<UFGAsset>();

            if (assetTag != null)
            {
                PopulateProperty(assetTag, property);
            }

        }



        private static void PopulateField(UFGAsset assetTag, FieldInfo field)
        {
            //Check if a key was provided to the attribute, otherwise use the member's name.
            string assetKey = (assetTag.Key != "") ? assetTag.Key : field.Name;

            HydraLogger.Log($"AssetLoader: Found asset {((field.FieldType.IsArray) ? "array " : "")}tag {assetKey}");

            if(field.FieldType.IsArray && !assetKey.Contains("{0}"))
            {
                HydraLogger.Log($"AssetLoader: Error if an asset tag is placed on an array the key must contain a placeholder bracket with 0 to be iteratable. Skipping.", DebugChannel.Fatal);
                return;
            }

            

            if (field.FieldType.IsArray)
            {
                Type fieldType = field.FieldType.GetElementType();

                List<UnityEngine.Object> loadedAssets = new List<UnityEngine.Object>();

                int counter = 0;
                while (TryLoadAsset(string.Format(assetKey, counter), HydraLoader.AssetBundle, fieldType, out UnityEngine.Object loadedObject))
                {
                    if (loadedObject != null)
                    {
                        loadedAssets.Add(loadedObject);
                    }
                    counter++;
                }

                if (loadedAssets.Count > 0)
                {
                    field.SetValue(null, loadedAssets.ToArray());
                    HydraLogger.Log($"AssetLoader: {assetKey} ({field.FieldType}), successfully cached to {field.DeclaringType}.{field.Name} with {loadedAssets.Count} indicies.");
                }

            }
            else
            {

                Type fieldType = field.FieldType;
                if (TryLoadAsset(assetKey, HydraLoader.AssetBundle, fieldType, out UnityEngine.Object loadedAsset))
                {
                    HydraLogger.Log($"AssetLoader: {assetKey} ({fieldType.Name}), successfully cached to {field.DeclaringType.Name}.{field.Name}");
                    field.SetValue(null, loadedAsset);
                }
                else
                {
                    HydraLogger.Log($"{field.DeclaringType.Name}:{field.Name}:CacheOnLoad: ({assetKey}) Load error, see above.", DebugChannel.Error);
                }
            }
        }

        private static void PopulateProperty(UFGAsset assetTag, PropertyInfo property)
        {
            //Check if a key was provided to the attribute, otherwise use the member's name.
            string assetKey = (assetTag.Key != "") ? assetTag.Key : property.Name;

            HydraLogger.Log($"AssetLoader: Found asset {((property.PropertyType.IsArray) ? "array " : "")}tag {assetKey}");


            if (!property.CanWrite)
            {
                HydraLogger.Log($"AssetLoader: {property.DeclaringType.Name}: {property.Name}: No setter found, unable to assign asset.", DebugChannel.Fatal);
                return;
            }


            if (property.PropertyType.IsArray && !assetKey.Contains("{0}"))
            {
                HydraLogger.Log($"AssetLoader: Error if an asset tag is placed on an array the key must contain a placeholder bracket with 0 to be iteratable. Skipping.", DebugChannel.Fatal);
                return;
            }

            

            if(property.PropertyType.IsArray)
            {
                Type propertyType = property.PropertyType.GetElementType();

                List<UnityEngine.Object> loadedAssets = new List<UnityEngine.Object>();

                int counter = 0;
                while (TryLoadAsset(string.Format(assetKey, counter), HydraLoader.AssetBundle, propertyType, out UnityEngine.Object loadedObject))
                {
                    if (loadedObject != null)
                    {
                        loadedAssets.Add(loadedObject);
                    }
                    counter++;
                }

                if (loadedAssets.Count > 0)
                {
                    property.SetValue(null, loadedAssets.ToArray());
                    HydraLogger.Log($"AssetLoader: {assetKey} ({property.PropertyType}), successfully cached to {property.DeclaringType}.{property.Name} with {loadedAssets.Count} indicies.");
                }

            }
            else
            {
                Type propertyType = property.PropertyType;

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
