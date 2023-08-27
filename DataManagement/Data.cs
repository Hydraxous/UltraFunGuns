using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Reflection;
using Newtonsoft.Json;
using System.ComponentModel;
using GameConsole;
using UltraFunGuns.Datas;
using HydraDynamics.DataPersistence;
using HydraDynamics;

namespace UltraFunGuns
{
    public static class Data
    {
        public static DataFile<Loadout> Loadout { get; private set; } = new DataFile<Loadout>(new Loadout(), "loadout.ufg");
        public static DataFile<SaveInfo> SaveInfo { get; private set; } = new DataFile<SaveInfo>(new SaveInfo(), "save.ufg");
        public static DataFile<Config> Config { get; private set; } = new DataFile<Config>(new Config(), "config.txt", Formatting.Indented);

        public static void SaveAll()
        {
            HydraLogger.Log("Saving all.", DebugChannel.User);
            Loadout.Save();
            SaveInfo.Save();
            Config.Save();
            HydraLogger.WriteLog();
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
            if (InGameCheck.InLevel())
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
            HydraLogger.Log("Creating new persistent data.", DebugChannel.User);
            Loadout.New();
            Config.New();
            SaveInfo.New();
        }

        public static void CheckSetup()
        {
            InGameCheck.OnLevelChanged += (_) => SaveAll();
            DirectoryInfo dataFolderInfo = new DirectoryInfo(DataManager.GetDataPath());
            if (dataFolderInfo.GetFiles().Length <= 0)
            {
                FirstTimeSetup();
                HydraLogger.Log($"Thanks for installing UltraFunGuns! I hope you enjoy my silly weapons. :) -Hydra", DebugChannel.User);
            }
        }

        public static string GetDataPath(params string[] subpath)
        {
            string asmFolder = Assembly.GetExecutingAssembly().Location;
            string localPath = Path.GetDirectoryName(asmFolder);

            if (!Directory.Exists(localPath))
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

    }


    
}



