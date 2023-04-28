using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;

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

        private T LoadAsset<T>() where T : UnityEngine.Object
        {
            UnityEngine.Object outputObject = Addressables.LoadAssetAsync<T>(path).WaitForCompletion();

            return (T) outputObject;
        }

        public UKAsset(string key) 
        {
            path = key;
        }
    }
}
