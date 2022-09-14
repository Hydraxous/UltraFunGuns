using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public static class HydraLoader
    {

        public static Dictionary<String, DataFile> dataRegistry = new Dictionary<String, DataFile>();

        public static Dictionary<String, AssetBundle> customAssetList = new Dictionary<String, AssetBundle>();

        public static Dictionary<String, GameObject> customPrefabs = new Dictionary<String, GameObject>();

        public static bool RegisterAll()
        {
            try
            {
                RegisterDataFiles();
                RegisterCustomAssets();
                RegisterCustomPrefabs();

                return true;
            }catch (Exception e)
            {
                return false;
            }
        }

        public static void RegisterDataFiles()
        {
            GyroRotator.GyroRotatorData innerGyroStabilizer = new GyroRotator.GyroRotatorData(1.0f,Vector3.forward, 0.004f,3f,40.66f); //InnerGyroBearing of SonicReverberator gun
            dataRegistry.Add("InnerGyroBearing", innerGyroStabilizer);

            GyroRotator.GyroRotatorData middleGyro = new GyroRotator.GyroRotatorData(1.2f, new Vector3(1, 0, 0), 0.004f, 3.5f, -53.58f); //MiddleGyro of SonicReverberator
            dataRegistry.Add("MiddleGyro", middleGyro);

            GyroRotator.GyroRotatorData innerGyro = new GyroRotator.GyroRotatorData(1.5f, Vector3.back, 0.004f, 4f, -134.3f); //InnerGyro of sonic reverbgun
            dataRegistry.Add("InnerGyro", innerGyro);

            GyroRotator.GyroRotatorData moyai = new GyroRotator.GyroRotatorData(1.5f, Vector3.one, 0.005f, 15f, -248.5f);
            dataRegistry.Add("Moyai", moyai);
        }

        public static void RegisterCustomAssets()
        {

        }

        public static void RegisterCustomPrefabs()
        {

        }

    }

    public class DataFile { }
}
