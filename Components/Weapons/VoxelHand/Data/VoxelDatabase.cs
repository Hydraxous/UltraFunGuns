using Configgy;
using MonoMod.Utils;
using System;
using System.Collections;
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

        public static IEnumerable<VoxelData> GetPlaceableVoxels()
        {
            if (voxelRegistry == null)
            {
                InitializeVoxelDatabase();
            }

            IEnumerable<VoxelData> data = voxelRegistry.Values;

            if(customBlocks != null)
                data = data.Concat(customBlocks.Values);

            return data;
        }


        [UFGAsset("boulder_impact_on_stones_14 fast")]
        private static AudioClip defaultClip;

        private static string customVoxelsFolder => HydraDynamics.DataPersistence.DataManager.GetDataPath("Voxel", "CustomVoxels");

        private static Dictionary<string, Texture2D> loadedTextures = new Dictionary<string,Texture2D>();
        
        public static void OpenCustomVoxelFolder()
        {
            Application.OpenURL($"file://{customVoxelsFolder}");
        }

        public static void ImportCustomBlocksAsync(Action onComplete)
        {
            if (IsImportingTextures)
                return;

            StaticCoroutine.RunCoroutine(ImportCustomTexturesAsync(() => { OnCustomBlocksUpdated?.Invoke(); }));
        }

        public static bool IsImportingTextures { get; private set; }
        public static float TextureImportProgress { get; private set; }

        [Configgable("TEST")]
        private static ConfigToggle useCyberGrindTextures = new ConfigToggle(false);
        private static string cyberGrindTextureFolder => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "CyberGrind", "Textures");

        private static IEnumerator ImportCustomTexturesAsync(Action onComplete)
        {
            IsImportingTextures = true;

            List<string> pathes = new List<string>();

            pathes.AddRange(Directory.GetFiles(customVoxelsFolder, "*.png", SearchOption.AllDirectories));
            pathes.AddRange(Directory.GetFiles(customVoxelsFolder, "*.jpg", SearchOption.AllDirectories));
            pathes.AddRange(Directory.GetFiles(customVoxelsFolder, "*.jpeg", SearchOption.AllDirectories));
            if (useCyberGrindTextures.Value)
            {
                pathes.AddRange(Directory.GetFiles(cyberGrindTextureFolder, "*.png", SearchOption.AllDirectories));
                pathes.AddRange(Directory.GetFiles(cyberGrindTextureFolder, "*.jpg", SearchOption.AllDirectories));
                pathes.AddRange(Directory.GetFiles(cyberGrindTextureFolder, "*.jpeg", SearchOption.AllDirectories));
            }

            int totalCount = pathes.Count+1; //stop dividebyzero exception
            int indexProcessed = 1;

            foreach (string path in pathes)
            {
                TextureImportProgress = (float)indexProcessed / ((float)totalCount*2);
                ++indexProcessed;

                if (loadedTextures.ContainsKey(path))
                    continue;

                yield return new WaitForEndOfFrame();

                if (!TextureLoader.TryLoadTexture(path, out Texture2D tex))
                    continue;

                if (tex.width != tex.height) //Squares only.
                    continue;

                tex.name = Path.GetFileNameWithoutExtension(path);
                loadedTextures.Add(path, tex);

                yield return new WaitForEndOfFrame();
            }

            Material defaultMaterial = VoxelMaterialManager.GetDefaultMaterial();

            foreach (Texture2D texture in loadedTextures.Values)
            {
                TextureImportProgress = (float)indexProcessed / ((float)totalCount * 2);
                ++indexProcessed;

                if (texture == null)
                    continue;

                if (customBlocks.ContainsKey(texture.name))
                {
                    //block is loaded, so skip it.
                    if (customBlocks[texture.name].Material != defaultMaterial)
                        continue;
                    
                    customBlocks.Remove(texture.name);
                }

                VoxelData customVoxelData = CreateCustomBlock(texture);
                customBlocks.Add(texture.name, customVoxelData);
            }

            indexProcessed = totalCount;
            TextureImportProgress = 1f;
            IsImportingTextures = false;
            onComplete?.Invoke();
        }

        public static event Action OnCustomBlocksUpdated;

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
            ImportCustomBlocksAsync(null);

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            ImportCustomBlocksAsync(null);
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
            return new VoxelData(id, id, VoxelMaterialManager.GetDefaultMaterial(), defaultClip);
        }
    }
}
