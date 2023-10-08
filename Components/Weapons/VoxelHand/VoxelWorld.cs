using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class VoxelWorld : MonoBehaviour
    {
        private static VoxelWorld _instance;

        [Configgy.Configgable("UltraFunGuns/Voxel/World")]
        private static float worldScale = 2f;

        [Configgy.Configgable("UltraFunGuns/Voxel/World")]
        private static int maxBlocks = 10000;

        public static float WorldScale => worldScale;
        public static int MaxBlocks => maxBlocks;

        private Dictionary<Vector3Int, Voxel> voxelData = new Dictionary<Vector3Int, Voxel>();

        private static VoxelWorld instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new GameObject("VoxelWorld").AddComponent<VoxelWorld>();
                }
                return _instance;
            }
        }

        private void ClearAllBlocks()
        {
            if (voxelData == null)
            {
                voxelData = new Dictionary<Vector3Int, Voxel>();
                return;
            }


            foreach (KeyValuePair<Vector3Int, Voxel> voxel in voxelData)
            {
                if (voxel.Value == null)
                    continue;

                Voxel vox = voxel.Value;

                if (vox != null)
                    GameObject.Destroy(vox.gameObject);
            }

            voxelData.Clear();
        }

        public static void ClearBlocks()
        {
            instance.ClearAllBlocks();
        }

        public static bool TryGetVoxelAtLocation(VoxelLocation location, out Voxel voxel)
        {
            voxel = null;

            if (!instance.voxelData.ContainsKey(location.Coordinate))
                return false;

            voxel = instance.voxelData[location.Coordinate];
            return voxel != null;
        }
        
        public static void SetVoxel(VoxelLocation position, Voxel voxel)
        {
            if (voxel == null)
            {
                if (instance.voxelData.ContainsKey(position.Coordinate))
                    instance.voxelData.Remove(position.Coordinate);
                return;
            }
                
            instance.voxelData[position.Coordinate] = voxel;
        }

        public static void ReplaceVoxel(VoxelLocation position, Voxel voxel)
        {
            DeleteVoxel(position);

            if (voxel == null)
                return;

            SetVoxel(position, voxel);
        }

        public static void DeleteVoxel(VoxelLocation location)
        {
            Vector3Int coord = location.Coordinate;

            if (!instance.voxelData.ContainsKey(coord))
                return;

            Voxel v = instance.voxelData[coord];
            
            if (v != null)
                GameObject.Destroy(v.gameObject);

            instance.voxelData.Remove(coord);
        }

        public static Voxel GetVoxelAtCoordinate(Vector3Int coordinate)
        {
            if (!instance.voxelData.ContainsKey(coordinate))
                return null;

            return instance.voxelData[coordinate];
        }

        public static Voxel GetVoxelAtPosition(Vector3 position)
        {
            Vector3Int coordinate = VoxelLocation.PositionToCoordinate(position);
            return GetVoxelAtCoordinate(coordinate);
        }

        public static Voxel GetVoxelAtLocation(VoxelLocation location)
        {
            if (!instance.voxelData.ContainsKey(location.Coordinate))
                return null;

            return instance.voxelData[location.Coordinate];
        }

        public static bool CheckVoxelCollision(Vector3 position)
        {
            Vector3Int coord = VoxelLocation.PositionToCoordinate(position);
            if (instance.voxelData.ContainsKey(coord))
                return instance.voxelData[coord] != null;

            float halfCube = VoxelWorld.WorldScale * 0.5f;
            Collider[] hitColliders = Physics.OverlapBox(position, Vector3.one * halfCube, Quaternion.identity, LayerMaskDefaults.Get(LMD.EnemiesAndPlayer), QueryTriggerInteraction.Ignore);
            return hitColliders.Length > 0;
        }

        public static void LoadWorld()
        {
            ClearBlocks();

            string sceneName = SceneHelper.CurrentScene;
            string folder = HydraDynamics.DataPersistence.DataManager.GetDataPath("Voxel", "Saves");
            string filePath = Path.Combine(folder, sceneName + worldFileExtension);

            if(!File.Exists(filePath))
            {
                HydraLogger.Log("Unable to find world save.", DebugChannel.Error);
                return;
            }

            VoxelWorldData current = null;

            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                current = (VoxelWorldData) binaryFormatter.Deserialize(file);
            }

            Debug.LogWarning($"Loading save!");
            PopulateWorld(current);
        }

        private static void PopulateWorld(VoxelWorldData worldData)
        {
            if (worldData == null)
                throw new NullReferenceException("World data is null!");

            worldScale = worldData.WorldScale;

            int index = 0;
            foreach (SerializedVoxel voxel in worldData.VoxelData)
            {
                VoxelLocation voxelLocation = new VoxelLocation(new Vector3Int(voxel.x, voxel.y, voxel.z));
                VoxelData voxelData = VoxelDatabase.GetVoxelData(voxel.id);

                if (voxelData == null)
                {
                    voxelData = VoxelDatabase.GetPlaceholderVoxelData(voxel.id);
                    VoxelDatabase.RegisterCustomVoxelData(voxelData);
                }

                IVoxelState voxelState = null;

                if (voxel.stateType != null)
                {
                    voxelState = (IVoxelState)Activator.CreateInstance(voxel.stateType);
                    voxelState.SetStateData(voxel.stateData);
                }

                //Debug.LogWarning($"[{index+1}] - - -\nID:{voxelData.ID}\nDN:{voxelData.DisplayName}\nC:{voxelLocation.Coordinate}\nP:{voxelLocation.Position}");

                Voxel.Create(voxelLocation, voxelData, voxelState);
                ++index;
            }
        }

        public static void SaveWorld()
        {
            CleanWorldData();
            SerializedVoxel[] serializedVoxels = new SerializedVoxel[instance.voxelData.Count];

            int index = 0;
            foreach (KeyValuePair<Vector3Int, Voxel> voxel in instance.voxelData)
            {
                Voxel logicVoxel = voxel.Value;
                Vector3Int coordinate = voxel.Key;
                
                if(logicVoxel != null)
                    serializedVoxels[index] = new SerializedVoxel(coordinate.x, coordinate.y, coordinate.z, logicVoxel.VoxelData.ID, logicVoxel.VoxelState);
                
                ++index;
            }

            string sceneName = SceneHelper.CurrentScene;

            VoxelWorldData worldData = new VoxelWorldData(sceneName, "World", worldScale, serializedVoxels);

            string folder = HydraDynamics.DataPersistence.DataManager.GetDataPath("Voxel", "Saves");
            string filePath = Path.Combine(folder, sceneName + worldFileExtension);

            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(file, worldData);
            }

            Debug.LogWarning($"Saved {sceneName}");

        }

        //voxel world data
        private const string worldFileExtension = ".vwd";

        private static void CleanWorldData()
        {
            if (instance.voxelData == null)
                return;

            instance.voxelData = instance.voxelData.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
        }

    }
}
