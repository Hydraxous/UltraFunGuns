using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using UltraFunGuns.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltraFunGuns
{
    public static class VoxelDatabase
    {
        [UFGAsset("vh_selection")] private static VoxelDataDef selection;
        public static VoxelData Selection => selection.VoxelData;

        [UFGAsset("VoxelBreakParticle")] private static GameObject voxelParticlePrefab;
        public static GameObject VoxelParticle => voxelParticlePrefab;


        private static VoxelData[] builtInVoxelData;

        private static Dictionary<string, VoxelData> voxelRegistry;
        private static Dictionary<string, VoxelData> customBlocks = new Dictionary<string, VoxelData>();

        //Blacklisted from being in the usable pool
        //TODO move this to the UI Code when able.
        private static readonly string[] blacklistedVoxels =
        {
            "vh_selection"
        };

        public static VoxelData[] GetPlaceableVoxels()
        {
            if(voxelRegistry == null)
            {
                InitializeVoxelDatabase();
            }

            List<VoxelData> voxels = voxelRegistry.Values.ToList();

            if(customBlocks != null)
            {
                voxels.AddRange(customBlocks.Values);
            }

            return voxels.ToArray();
        }


        [UFGAsset("boulder_impact_on_stones_14 fast")]
        private static AudioClip defaultClip;

        [UFGAsset("cobblestone")]
        private static Texture2D defaultTexture;


        private static string customVoxelsFolder => HydraDynamics.DataPersistence.DataManager.GetDataPath("Voxel", "CustomVoxels");

        private static Dictionary<string, Texture2D> loadedTextures = new Dictionary<string,Texture2D>();
        
        public static void OpenCustomVoxelFolder()
        {
            Application.OpenURL($"file://{customVoxelsFolder}");
        }

        public static void ImportCustomBlocks()
        {
            List<string> pathes = new List<string>();
            pathes.AddRange(Directory.GetFiles(customVoxelsFolder, "*.png"));
            pathes.AddRange(Directory.GetFiles(customVoxelsFolder, "*.jpg"));

            foreach (string path in pathes)
            {
                if (loadedTextures.ContainsKey(path))
                    continue;

                if (!TextureLoader.TryLoadTexture(path, out Texture2D tex))
                    continue;

                tex.name = Path.GetFileNameWithoutExtension(path);
                loadedTextures.Add(path, tex);
            }

            foreach (Texture2D texture in loadedTextures.Values)
            {
                if (texture == null)
                    continue;

                if(customBlocks.ContainsKey(texture.name))
                {
                    Debug.LogWarning($"Duplicate texture found of name {texture.name}");
                    continue;
                }

                VoxelData customVoxelData = CreateCustomBlock(texture);
                customBlocks.Add(texture.name, customVoxelData);
            }
        }

        private static void ImportPackagedVoxels()
        {
            if (builtInVoxelData != null)
                return;

            if (HydraLoader.AssetBundle == null)
                return;

            builtInVoxelData = HydraLoader.AssetBundle.LoadAllAssets<VoxelDataDef>().Where(x => !blacklistedVoxels.Contains(x.name)).Select(x => ((VoxelDataDef)x).VoxelData).ToArray();
            voxelRegistry = builtInVoxelData.ToDictionary(x => x.ID, x=>x);
        }


        private static VoxelData CreateCustomBlock(Texture2D texture)
        {
            texture.filterMode = FilterMode.Point;
            VoxelData voxelData = new VoxelData(texture.name, texture.name, VoxelMaterialManager.GetMaterial(texture), defaultClip);
            return voxelData;
        }

        public static void RegisterCustomVoxelData(VoxelData voxelData)
        {
            if (voxelData == null)
                throw new ArgumentNullException(nameof(voxelData));

            if(customBlocks.ContainsKey(voxelData.ID))
                throw new DuplicateNameException(nameof(voxelData));

            customBlocks.Add(voxelData.ID, voxelData);
        }

        private static void InitializeVoxelDatabase()
        {
            ImportPackagedVoxels();
            ImportCustomBlocks();

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            ImportCustomBlocks();
        }

        public static VoxelData GetVoxelData(string id)
        {
            if (voxelRegistry == null)
                InitializeVoxelDatabase();

            if(voxelRegistry.ContainsKey(id))
                return voxelRegistry[id];

            if(customBlocks.ContainsKey(id))
                return customBlocks[id];

            return null;
        }

        //Used for custom blocks, fe they dont exist anymore.
        public static VoxelData GetPlaceholderVoxelData(string id)
        {
            return new VoxelData(id, id, VoxelMaterialManager.GetMaterial(defaultTexture), defaultClip);
        }
    }
}
