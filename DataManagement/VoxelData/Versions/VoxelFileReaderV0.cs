using System.IO;

namespace UltraFunGuns
{
    public class VoxelFileReaderV0 : IVoxelFileReader
    {
        public VoxelWorldFileHeader ReadHeader(BinaryReader br)
        {
            VoxelWorldFileHeader header = new VoxelWorldFileHeader();
            header.TotalVoxelCount = br.ReadInt32();
            header.WorldScale = br.ReadSingle();
            header.SceneName = br.ReadString();
            header.DisplayName = br.ReadString();
            header.Description = br.ReadString();
            header.ModVersion = br.ReadString();
            header.GameVersion = br.ReadString();
            return header;
        }

        public VoxelWorldFile ReadWorldData(BinaryReader br)
        {
            VoxelWorldFile data = new VoxelWorldFile();
            data.Header = ReadHeader(br);
            data.VoxelData = new SerializedVoxel[data.Header.TotalVoxelCount];
            for (int i = 0; i < data.Header.TotalVoxelCount; i++)
            {
                data.VoxelData[i] = ReadVoxel(br);
            }
            return data;
        }

        public SerializedVoxel ReadVoxel(BinaryReader br)
        {
            SerializedVoxel voxel = new SerializedVoxel();
            voxel.x = br.ReadInt32();
            voxel.y = br.ReadInt32();
            voxel.z = br.ReadInt32();
            voxel.id = br.ReadString();
            voxel.stateTypeID = br.ReadString();
            int dataLength = br.ReadInt32();
            voxel.stateData = br.ReadBytes(dataLength);
            return voxel;
        }
    }
}
