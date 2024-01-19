using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public static class Paths
    {
        public static string GameFolder => Directory.GetParent(Application.dataPath).FullName;
        public static string ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string DataFolder => Path.Combine(ModFolder, "Data");
        public const string SAVE_FILE_EXTENSION = ".ufg";
        public static string SaveFilePath => Path.Combine(UFGSlotDataFolder, "save"+SAVE_FILE_EXTENSION);


        public const string VOXEL_SAVE_FILE_EXTENSION = ".vwd";
        public static string VoxelDataFolder => Path.Combine(DataFolder, "Voxel");
        public static string VoxelTexturesFolder => Path.Combine(VoxelDataFolder, "Textures");
        public static string VoxelBlockTexturesFolder => Path.Combine(VoxelTexturesFolder, "Block");
        public static string VoxelPlayerTexturesFolder => Path.Combine(VoxelTexturesFolder, "PlayerSkin");
        public static string VoxelSavesFolder => Path.Combine(UFGGlobalDataFolder, "VoxelSaves");


        public static string GameSavesFolder => GameProgressSaver.BaseSavePath;
        public static string UFGGlobalDataFolder => Path.Combine(GameSavesFolder, ConstInfo.NAME);
        public static string UFGSlotDataFolder => Path.Combine(CurrentSaveSlotFolder, ConstInfo.NAME);


        public static string BepInExFolder => Path.Combine(GameFolder, "BepInEx");

        public static string CybergrindFolder => Path.Combine(GameFolder, "Cybergrind");
        public static string CybergrindTexturesFolder => Path.Combine(CybergrindFolder, "Textures");

        public static string CurrentSaveSlotFolder => GameProgressSaver.SavePath;
    }
}
