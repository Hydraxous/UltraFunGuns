﻿using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Reflection;
using Newtonsoft.Json;
using System.ComponentModel;

namespace UltraFunGuns
{
    public static class Data
    {
        private const string folderName = "UFG_Data";
        private static string fileExtension = ".ufg";

        public static bool AutoSave = true;

        public static UFGPersistentData<UFG_Configuration> Config = new UFGPersistentData<UFG_Configuration>();
        public static UFGPersistentData<UFG_SaveData> Save = new UFGPersistentData<UFG_SaveData>();
        public static UFGPersistentData<UFG_Loadout> Loadout = new UFGPersistentData<UFG_Loadout>();

        public static string GetDataPath(params string[] subpath)
        {
            string modDir = Assembly.GetExecutingAssembly().Location;
            modDir = Path.GetDirectoryName(modDir);
            string localPath = Path.Combine(modDir, folderName);

            if(!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }

            if (subpath.Length > 0)
            {
                string subLocalPath = Path.Combine(subpath);
                localPath = Path.Combine(localPath, subLocalPath);
            }

            return localPath;
        }

        public static void SaveAll()
        {
            Loadout.Save();
            Save.Save();
            Config.Save();
        }

        [Commands.UFGDebugMethod("Reload Config","Reloads the config file.")]
        public static void ReloadConfig()
        {
            Config.Load();
        }

        [Commands.UFGDebugMethod("Reset Loadout", "Resets UFG loadout to default.")]
        public static void ResetLoadout()
        {
            Loadout.New();
            if(UKAPIP.InLevel())
            {
                WeaponManager.DeployWeapons();
            }
        }

        [Commands.UFGDebugMethod("Reset Save Data", "Resets UFG save data to default.")]
        public static void ResetSaveData()
        {
            Save.New();
        }

        [Commands.UFGDebugMethod("Reset Config", "Resets UFG config to default.")]
        public static void ResetConfig()
        {
            Config.New();
        }

        private static void SaveData<T>(T dataObject) where T : UFGData
        {
            string serializedData = JsonConvert.SerializeObject(dataObject, dataObject.JsonFormat());
            string dataFilePath = GetDataPath(dataObject.FileName() + dataObject.FileExtension());
            File.WriteAllText(dataFilePath, serializedData);
            HydraLogger.Log($"{dataObject.FileName()} Data saved.");
        }

        private static T LoadData<T>() where T : UFGData, new()
        {
            T dataObject = new T();

            string dataFilePath = GetDataPath(dataObject.FileName() + dataObject.FileExtension());


            if (File.Exists(dataFilePath))
            {
                string jsonData;
                using (StreamReader reader = new StreamReader(dataFilePath))
                {
                    jsonData = reader.ReadToEnd();
                }

                dataObject = JsonConvert.DeserializeObject<T>(jsonData);
                try
                {
                    if (dataObject.IsValid())
                        return dataObject;
                }catch (Exception ex)
                {
                    //HydraLogger.Log($"Data object invalid!\n{ex.Message}\n{ex.StackTrace}", DebugChannel.Fatal);
                }
                
            }

            dataObject = new T();
            //HydraLogger.Log($"Loaded: {dataObject.FileName()}");
            SaveData(dataObject);
            return dataObject;
        }

        public static void CheckSetup()
        {
            DirectoryInfo dataFolderInfo = new DirectoryInfo(GetDataPath());
            if(!(dataFolderInfo.GetFiles().Length > 0))
            {
                HydraLogger.Log($"Thanks for installing UltraFunGuns! I hope you enjoy my silly weapons. -Hydra", DebugChannel.User);
                FirstTimeSetup();
            }
        }

        [Commands.UFGDebugMethod("Reset All Data", "Resets UFG Mod data.")]
        public static void FirstTimeSetup()
        {
            HydraLogger.Log("Creating new persistent data.");
            Loadout.New();
            Config.New();
            Save.New();
        }

        public delegate void OnDataChangedHandler();
        public static OnDataChangedHandler OnDataChanged;


        public static UFGWeapon GetWeaponInfo(Type t)
        {
            UFGWeapon weaponInfo = (UFGWeapon) Attribute.GetCustomAttribute(t, typeof(UFGWeapon));
            
            if(weaponInfo == null)
            {
                HydraLogger.Log($"Weapon info null when requested for type {t.ToString()}", DebugChannel.Fatal);
            }

            return weaponInfo;
        }

        [System.Serializable]
        public class UFG_Loadout : UFGData
        {

            public InventorySlotData[] slots;

            public UFG_Loadout(InventorySlotData[] slots)
            {
                this.slots = slots;
            }

            public UFG_Loadout()
            {
                this.slots = WeaponManager.GetDefaultLoadout();
            }

            public override bool IsValid()
            {
                if (!(slots.Length > 0))
                {
                    return false;
                }

                int wepCounter = 0;
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i] == null)
                    {
                        return false;
                    }

                    if (slots[i].slotNodes.Length > 0)
                    {
                        wepCounter += slots[i].slotNodes.Length;
                    }

                    if (!(wepCounter > 0))
                    {
                        return false;
                    }
                }

                if(wepCounter != WeaponManager.WeaponCount)
                {
                    return false;
                }

                return true;
            }

            public override string FileName() { return "loadout"; }
        }

        [System.Serializable]
        public class UFG_Configuration : UFGData
        {
            //Generic
            public bool DebugMode;
            public bool DisableVersionMessages;
            public bool EnableVisualizer;

            //Weapon values
            public bool BasketBallMode;

            //UI
            public float InventoryInfoCardScale;
            public float MouseOverNodeTime;

            public UFG_Configuration()
            {
                this.DisableVersionMessages = false;
                this.BasketBallMode = false;
                this.InventoryInfoCardScale = 1.0f;
                this.MouseOverNodeTime = 0.3f;
                this.DebugMode = false;
                this.EnableVisualizer = false;
            }

            public override string FileName() { return "config"; }
            public override string FileExtension() { return ".txt"; }
            public override Formatting JsonFormat() { return Formatting.Indented; }

            public override bool IsValid()
            {
                return true;
            }

        }

        [System.Serializable]
        public class UFG_SaveData : UFGData
        {
            public bool firstTimeUsingInventory;
            public bool firstTimeModLoaded;
            public string modVersion;

            public UFG_SaveData()
            {
                this.modVersion = UltraFunGuns.RELEASE_VERSION;
                this.firstTimeModLoaded = true;
                this.firstTimeUsingInventory = true;
            }

            public override string FileName() { return "save"; }

            public override bool IsValid()
            {
                if (modVersion == null)
                {
                    return false;
                }

                if (modVersion == "")
                {
                    return false;
                }

                //If the version is mismatched with the save files, regenerate all files.
                if(modVersion != UltraFunGuns.RELEASE_VERSION)
                {
                    Loadout.New();
                    Config.New();
                    return false;
                }

                return true;
            }
        }

        [System.Serializable]
        public abstract class UFGData
        {
            public abstract bool IsValid();
            public abstract string FileName();
            public virtual string FileExtension() { return fileExtension; }
            public virtual Formatting JsonFormat() { return Formatting.None; }
        }
        
        //TODO make modified properties serialize correctly. You must call save after modifying members, very annoying.

        public class UFGPersistentData<T> where T : UFGData, new()
        {
            private T data;

            public T Data
            {
                get
                {
                    if (data == null)
                    {
                        Load();
                    }
                    return data;
                }

                set
                {
                    HydraLogger.Log($"Data was changed somewhat!!!!! {data.FileName()}");

                    if (data != value && value != null)
                    {
                        if (value.IsValid())
                        {
                            data = value;
                            OnDataChanged?.Invoke();
                            HydraLogger.Log($"Data was changed differentlyyyy!!!!! {data.FileName()}");
                            if (AutoSave)
                            {
                                Save();
                            }
                            return;
                        }
                    }
                }
            }

            public UFGPersistentData()
            {
                Load();
            }

            public void Save()
            {
                SaveData(data);
            }

            public void Load()
            {
                data = LoadData<T>();
            }

            public void New()
            {
                data = new T();
                Save();
            }

        }

    }
}


