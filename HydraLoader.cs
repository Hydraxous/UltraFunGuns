using System;
using System.Collections.Generic;
using UnityEngine;
using UltraFunGuns.Properties;

namespace UltraFunGuns
{
    public static class HydraLoader
    {

        public static Dictionary<string, DataFile> dataRegistry = new Dictionary<string, DataFile>();

        public static Dictionary<string, GameObject> prefabRegistry = new Dictionary<string, GameObject>();

        public static bool dataRegistered = false;
        public static bool assetsRegistered = false;

        public static bool RegisterAll()
        {
            try
            {
                RegisterDataFiles();
                RegisterCustomAssets();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static void RegisterDataFiles()
        {
            if (!dataRegistered)
            {
                GyroRotator.GyroRotatorData innerGyroStabilizer = new GyroRotator.GyroRotatorData(1.0f, Vector3.forward, 0.004f, 3f, 40.66f); //InnerGyroBearing of SonicReverberator gun
                dataRegistry.Add("InnerGyroBearing", innerGyroStabilizer);

                GyroRotator.GyroRotatorData middleGyro = new GyroRotator.GyroRotatorData(1.2f, new Vector3(1, 0, 0), 0.004f, 3.5f, -53.58f); //MiddleGyro of SonicReverberator
                dataRegistry.Add("MiddleGyro", middleGyro);

                GyroRotator.GyroRotatorData innerGyro = new GyroRotator.GyroRotatorData(1.5f, Vector3.back, 0.004f, 4f, -134.3f); //InnerGyro of sonic reverbgun
                dataRegistry.Add("InnerGyro", innerGyro);

                GyroRotator.GyroRotatorData moyai = new GyroRotator.GyroRotatorData(1.5f, Vector3.one, 0.005f, 15f, -248.5f);
                dataRegistry.Add("Moyai", moyai);

                dataRegistered = true;
                Debug.Log("data registered!");
            }
        }

        public static void RegisterCustomAssets()
        {
            if (!assetsRegistered)
            {
                AssetBundle assetBundle = AssetBundle.LoadFromMemory(UltraFunGunsResources.UltraFunGuns); //CHANGE TO LOAD FROM MEMORY AND USE RESOURCES.

                //Moyai gun fx projectile.
                GameObject sonicReverberation = assetBundle.LoadAsset<GameObject>("SonicReverberation");
                SonicReverberatorExplosion reverberation = sonicReverberation.AddComponent<SonicReverberatorExplosion>();
                prefabRegistry.Add("SonicReverberation", sonicReverberation);

                //Moyai Gun
                GameObject sonicReverberator = assetBundle.LoadAsset<GameObject>("SonicReverberator");
                SonicReverberator sonicGun = sonicReverberator.AddComponent<SonicReverberator>();
                WeaponIcon srIcon = sonicReverberator.AddComponent<WeaponIcon>();
                srIcon.weaponIcon = assetBundle.LoadAsset<Sprite>("SonicReverberator_weaponIcon");
                srIcon.glowIcon = assetBundle.LoadAsset<Sprite>("SonicReverberator_glowIcon");
                sonicGun.vB_standard = assetBundle.LoadAsset<AudioClip>("vineBoom_Standard");
                sonicGun.vB_loud = assetBundle.LoadAsset<AudioClip>("vineBoom_Loud");
                sonicGun.vB_loudest = assetBundle.LoadAsset<AudioClip>("vineBoom_Loudest");
                srIcon.variationColor = 0;
                sonicReverberator.SetActive(false);
                sonicGun.bang = sonicReverberation;
                
                prefabRegistry.Add("SonicReverberator", sonicReverberator);

                assetsRegistered = true;
                Debug.Log("assets registered!");
            }
        }
    }

    public class DataFile { }
}
