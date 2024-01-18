using Configgy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UltraFunGuns
{
    public class VoxelWorld : MonoBehaviour
    {
        private static VoxelWorld _instance;

        [Configgy.Configgable("UltraFunGuns/Voxel/World")]
        private static ConfigInputField<float> worldScale = new ConfigInputField<float>(2f, ValidateWorldScaleInput);

        [Configgy.Configgable("UltraFunGuns/Voxel/World")]
        private static int maxBlocks = 10000;

        public static float WorldScale => worldScale.Value;
        public static int MaxBlocks => maxBlocks;

        private Dictionary<Vector3Int, Voxel> voxelData = new Dictionary<Vector3Int, Voxel>();
        private bool worldDirty;

        public static VoxelWorldFile CurrentFile { get; private set; }

        internal static void SetCurrentFile(VoxelWorldFile file)
        {
            CurrentFile = file;
        }

        public static bool IsWorldDirty()
        {
            return instance.worldDirty;
        }

        private static bool ValidateWorldScaleInput(float value)
        {
            return value > 0f;
        }

        private static VoxelWorld instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("VoxelWorld").AddComponent<VoxelWorld>();
                }
                return _instance;
            }
        }

        private void Start()
        {
            if(_instance != null && _instance != this)
            {
                Debug.LogError("VoxelWorld already exists! Destroying new instance.");
                DestroyImmediate(this);
                return;
            }

            if (CurrentFile != null)
            {
                Debug.Log("Populating existing voxel world.");
                PopulateWorld(CurrentFile);
            }

            worldScale.OnValueChanged += OnWorldScaleChanged;
        }

        //lol
        public static bool QueryInstance()
        {
            return instance != null;
        }

        private void OnWorldScaleChanged(float newValue)
        {
            foreach (KeyValuePair<Vector3Int, Voxel> voxel in voxelData)
            {
                if (voxel.Value == null)
                    continue;

                Voxel vox = voxel.Value;
                vox.transform.position = VoxelLocation.CoordinateToPosition(voxel.Key);
                vox.transform.localScale = Vector3.one * VoxelWorld.WorldScale;
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
            worldDirty = true;
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
            SetVoxelInternal(position, voxel);
            instance.worldDirty = true;
        }

        private static void SetVoxelInternal(VoxelLocation position, Voxel voxel)
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
            instance.worldDirty = true;
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

        public static void LoadWorld(VoxelWorldFile data)
        {
            if(data == null)
            {
                Debug.LogError($"VoxelWorld: parameter data is null! load aborted.");
                return;
            }

            ClearBlocks();
            instance.worldDirty = false;
            CurrentFile = data;
            PopulateWorld(data);
        }

        private static void PopulateWorld(VoxelWorldFile worldData)
        {
            if (worldData == null)
            {
                //throw new NullReferenceException("World data is null!");
                Debug.LogError("No world data ahh boy!");
            }
            //worldScale.SetValue(worldData.Header.WorldScale);

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

                if (string.IsNullOrEmpty(voxel.stateTypeID) && voxel.stateData != null && voxel.stateData.Length > 0)
                {
                    voxelState = (IVoxelState)Activator.CreateInstance(VoxelStateDatabase.GetStateType(voxel.stateTypeID));
                    voxelState.ReadStateData(new BinaryReader(new MemoryStream(voxel.stateData)));
                }

                Voxel newVoxel = Voxel.Create(voxelLocation, voxelData, voxelState);
                SetVoxelInternal(voxelLocation, newVoxel);
                ++index;
            }
        }

        public static void SaveCurrentWorld()
        {
            if(CurrentFile == null)
            {
                if (instance.worldDirty)
                {
                    //File not yet saved, prompt user to save as.
                    ModalDialogue.ShowSimple("No save selected.", "Would you like to save the current world as a new file?", (r) =>
                    {
                        if (r)
                        {
                            //Open save as menu and prompt for file name.
                            VoxelSelectionMenu menu = GameObject.FindObjectOfType<VoxelSelectionMenu>();
                            if(menu == null)
                            {
                                Debug.LogError("No voxel selection menu?!?!");
                                return;
                            }

                            menu.NewWorld();
                        }
                    });
                }
                return;
            }

            CurrentFile.VoxelData = SerializeCurrentVoxels();
            VoxelSaveManager.SaveWorldData(CurrentFile.Header.FilePath, CurrentFile);
            instance.worldDirty = false;
        }

        public static VoxelWorldFile BuildNewWorldFile(string displayName)
        {
            VoxelWorldFile data = new VoxelWorldFile();
            data.VoxelData = SerializeCurrentVoxels();

            data.Header = new VoxelWorldFileHeader();
            data.Header.DisplayName = displayName;
            data.Header.WorldScale = WorldScale;
            data.Header.Description = "My Voxel World!";
            data.Header.SceneName = SceneHelper.CurrentScene;
            data.Header.TotalVoxelCount = data.VoxelData.Length;
            data.Header.ModVersion = ConstInfo.VERSION;
            data.Header.GameVersion = Application.version;
            
            return data;
        }

        public static SerializedVoxel[] SerializeCurrentVoxels()
        {
            CleanWorldData();
            SerializedVoxel[] voxels = new SerializedVoxel[instance.voxelData.Count];

            int index = 0;
            foreach (KeyValuePair<Vector3Int, Voxel> voxel in instance.voxelData)
            {
                Voxel logicVoxel = voxel.Value;
                Vector3Int coordinate = voxel.Key;

                SerializedVoxel sVoxel = new SerializedVoxel();
                sVoxel.id = logicVoxel.VoxelData.ID;
                sVoxel.x = coordinate.x;
                sVoxel.y = coordinate.y;
                sVoxel.z = coordinate.z;
                sVoxel.stateTypeID = (logicVoxel.VoxelState == null) ? "" : logicVoxel.VoxelState.GetID();

                if (logicVoxel.VoxelState == null)
                {
                    sVoxel.stateData = Array.Empty<byte>();
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (BinaryWriter bw = new BinaryWriter(ms))
                        {
                            logicVoxel.VoxelState.WriteStateData(bw);
                            sVoxel.stateData = ms.ToArray();
                        }
                    }
                }

                voxels[index] = sVoxel;
                ++index;
            }

            return voxels;
        }

        private static void CleanWorldData()
        {
            if (instance.voxelData == null)
            {
                instance.voxelData = new Dictionary<Vector3Int, Voxel>();
                return;
            }

            instance.voxelData = instance.voxelData.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
        }

        private void OnDestroy()
        {
            _instance = null;
            worldScale.OnValueChanged -= OnWorldScaleChanged;
        }

    }
}
