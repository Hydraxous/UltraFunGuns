using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UltraFunGuns
{
    public class VoxelFileReaderV0 : IVoxelFileReader
    {
        public VoxelWorldFileHeader ReadHeader(BinaryReader br)
        {
            VoxelWorldFileHeader header = new VoxelWorldFileHeader();
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
