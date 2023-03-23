using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Reflection;
using Newtonsoft.Json;
using System.ComponentModel;
using GameConsole;
using UltraFunGuns.Datas;

namespace UltraFunGuns
{

    //TODO Redo this whole thing please. its kind of cursed. Done :)
    public static class Data
    {

        public static DataFile<Loadout> Loadout { get; private set; } = new DataFile<Loadout>(new Loadout(), "loadout.ufg");
        public static DataFile<SaveInfo> SaveInfo { get; private set; } = new DataFile<SaveInfo>(new SaveInfo(), "save.ufg");
        public static DataFile<Config> Config { get; private set; } = new DataFile<Config>(new Config(), "config.txt", Formatting.Indented);

        public static void SaveAll()
        {
            Loadout.Save();
            SaveInfo.Save();
            Config.Save();
        }

        [Commands.UFGDebugMethod("Reload Config", "Reloads the config file.")]
        public static void ReloadConfig()
        {
            Config.Load();
        }

        [Commands.UFGDebugMethod("Reset Loadout", "Resets UFG loadout to default.")]
        public static void ResetLoadout()
        {
            Loadout.New();
            if (UKAPIP.InLevel())
            {
                WeaponManager.DeployWeapons();
            }
        }

        [Commands.UFGDebugMethod("Reset Save Data", "Resets UFG save data to default.")]
        public static void ResetSaveData()
        {
            SaveInfo.New();
        }

        [Commands.UFGDebugMethod("Reset Config", "Resets UFG config to default.")]
        public static void ResetConfig()
        {
            Config.New();
        }

        [Commands.UFGDebugMethod("Reset All Data", "Resets UFG Mod data.")]
        public static void FirstTimeSetup()
        {
            HydraLogger.Log("Creating new persistent data.");
            Loadout.New();
            Config.New();
            SaveInfo.New();
        }

        public static void CheckSetup()
        {
            UKAPIP.OnLevelChanged += (_) => SaveAll();
            DirectoryInfo dataFolderInfo = new DirectoryInfo(DataManager.GetDataPath());
            if (dataFolderInfo.GetFiles().Length <= 0)
            {
                FirstTimeSetup();
                HydraLogger.Log($"Thanks for installing UltraFunGuns! I hope you enjoy my silly weapons. :) -Hydra", DebugChannel.User);
            }
        }
    }

    [System.Serializable]
    public class Loadout : Validatable
    {
        public InventorySlotData[] slots;

        public Loadout(InventorySlotData[] slots)
        {
            this.slots = slots;
        }

        public Loadout()
        {
            this.slots = WeaponManager.GetDefaultLoadout();
        }

        public override bool Validate()
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

            if (wepCounter != WeaponManager.WeaponCount)
            {
                return false;
            }

            return true;
        }
    }

    [System.Serializable]
    public class SaveInfo : Validatable
    {
        public bool firstTimeUsingInventory;
        public bool firstTimeModLoaded;
        public string modVersion;

        public SaveInfo()
        {
            this.modVersion = UltraFunGuns.RELEASE_VERSION;
            this.firstTimeModLoaded = true;
            this.firstTimeUsingInventory = true;
        }


        public override bool Validate()
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
            if (modVersion != UltraFunGuns.RELEASE_VERSION)
            {
                //DataManager.Config.New();
                //DataManager.Loadout.New();
                //DataManager.Keybinds.New();
                return false;
            }

            return true;
        }
    }

    [System.Serializable]
    public class Config : Validatable
    {
        //Generic
        public bool DebugMode;
        public bool DisableVersionMessages;
        public bool EnableVisualizer;

        //Weapon values
        public bool BasketBallMode;

        //UI
        public float MouseOverNodeTime;
        public float InventoryInfoCardScale;
        public Config()
        {
            this.DisableVersionMessages = false;
            this.BasketBallMode = false;
            this.MouseOverNodeTime = 0.8f;
            this.DebugMode = false;
            this.EnableVisualizer = false;
            this.InventoryInfoCardScale = 1.0f;
        }

        public override bool Validate()
        {
            return true;
        }
    }
}



