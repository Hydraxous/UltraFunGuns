using System.IO;
using System.Reflection;
using UnityEngine;

namespace UltraFunGuns
{
    internal static class Paths
    {
        public static string GameFolder => Directory.GetParent(Application.dataPath).FullName;
        public static string ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string DataFolder => Path.Combine(BepInExConfigFolder, ConstInfo.NAME);

        public static string LegacyDataFolder => Path.Combine(ModFolder, "Data");

        public static string BepInExFolder => Path.Combine(GameFolder, "BepInEx");
        public static string BepInExConfigFolder => Path.Combine(BepInExFolder, "config");

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


        public static string CybergrindFolder => Path.Combine(GameFolder, "Cybergrind");
        public static string CybergrindTexturesFolder => Path.Combine(CybergrindFolder, "Textures");

        public static string CurrentSaveSlotFolder => GameProgressSaver.SavePath;

        public static void CheckFolders()
        {
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);


            if (Directory.Exists(LegacyDataFolder))
            {
                Debug.LogWarning("Found legacy UFG data. Moving to new location.");
                CopyFilesRecursively(LegacyDataFolder, DataFolder);
                Debug.LogWarning("Legacy data moved.");
                Directory.Delete(LegacyDataFolder, true);
            }
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }
    }
}
