using Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace UltraFunGuns
{
    public class VoxelWorldFile
    {
        public const int FILE_VERSION = 0;
        public VoxelWorldFileHeader Header;
        public SerializedVoxel[] VoxelData;

        public BinaryWriter WriteBytes(BinaryWriter bw)
        {
            Header.TotalVoxelCount = VoxelData.Length;
            Header.WriteBytes(bw);
            for (int i = 0; i < VoxelData.Length; i++)
            {
                VoxelData[i].WriteBytes(bw);
            }
            return bw;
        }
    }

    public static class VoxelWorldFileExtensions
    {
        
    }

    public class VoxelWorldFileHeader
    {
        //Not serialized
        public string FilePath;

        public int TotalVoxelCount;
        public float WorldScale;

        public string SceneName;
        public string DisplayName;
        public string Description;
        public string ModVersion;
        public string GameVersion;

        public BinaryWriter WriteBytes(BinaryWriter bw)
        {
            bw.Write(TotalVoxelCount);
            bw.Write(WorldScale);
            bw.Write(SceneName);
            bw.Write(DisplayName);
            bw.Write(Description);
            bw.Write(ModVersion);
            bw.Write(GameVersion);
            return bw;
        }
    }
}
