﻿using System;
using System.Collections;
using UnityEngine;

namespace UltraFunGuns
{
    public static class UKPrefabs
    {
        private static AssetBundle gameAssets;

        public static AssetBundle GameAssets
        {
            get
            {
                if(gameAssets == null && !attemptingLoad)
                {
                    LoadAll();
                }

                return gameAssets;
            }
        }
        private static GameObject assetLoaderObject;

        private static bool attemptingLoad;

        public static bool AssetsLoaded
        {
            get
            {
                if(gameAssets == null)
                {
                    if(!attemptingLoad)
                    {
                        LoadAll();
                    }
                }
                return gameAssets != null;
            }
        }

        public static void LoadAll()
        {
            return;
            assetLoaderObject = new GameObject("UFG Game Asset Loader");
            GameObject.DontDestroyOnLoad(assetLoaderObject);
            assetLoaderObject.AddComponent<AnimatedPart>().StartCoroutine(LoadAssets());
        }

        private static IEnumerator LoadAssets()
        {
            attemptingLoad= true;

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Environment.CurrentDirectory + "\\ULTRAKILL_Data\\StreamingAssets\\common");

            yield return request;

            int attempts = 0;

            while (request.assetBundle == null)
            {
                yield return new WaitForSeconds(0.2f);
                if (attempts >= 5)
                {
                    UltraFunGuns.Log.LogError($"Couldn't load the game bundle.");
                    yield break;
                }
                request = AssetBundle.LoadFromFileAsync(Environment.CurrentDirectory + "\\ULTRAKILL_Data\\StreamingAssets\\common");
                yield return request;
                attempts++;
            }

            gameAssets = request.assetBundle;

            attemptingLoad = false;

            if (assetLoaderObject != null)
            {
                GameObject.Destroy(assetLoaderObject);
            }
        }

        public static T AssetFind<T>(string name) where T : UnityEngine.Object
        {
            T result = default;
            T[] allAssets = Resources.FindObjectsOfTypeAll<T>();

            foreach (T asset in allAssets)
                if (asset.name == name) result = asset;
            if (result == default)
                UltraFunGuns.Log.Log($"Failed to find asset: {name} of type {typeof(T)}");
            return result;
        }

        public static bool TryFindAsset(Type type, string name, out UnityEngine.Object outasset)
        {
            outasset = null;
            UnityEngine.Object[] allAssets = Resources.FindObjectsOfTypeAll(type);

            foreach (UnityEngine.Object asset in allAssets)
            {
                if (asset.name == name)
                {
                    outasset = asset;
                    return true;
                }
            }

            UltraFunGuns.Log.LogError($"Could not find asset : {name}");

            return false;
        }
    }
}
