using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace UltraFunGuns
{
    [Serializable]
    public class VoxelWorldData : ISerializable
    {
        public string SceneName { get; }
        public string DisplayName { get; }
        public SerializedVoxel[] VoxelData { get; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Scene", SceneName);
            info.AddValue("Name", DisplayName);
            info.AddValue("Data", VoxelData);
        }

        protected VoxelWorldData(SerializationInfo info, StreamingContext context) 
        {
            SceneName = info.GetString("Scene");
            DisplayName = info.GetString("Name");
            VoxelData = (SerializedVoxel[]) info.GetValue("Data", typeof(SerializedVoxel[]));
        }

        public VoxelWorldData(string sceneName, string displayName, SerializedVoxel[] voxelData) 
        {
            SceneName = sceneName;
            DisplayName = displayName;
            VoxelData = voxelData;
        }
    }
}
