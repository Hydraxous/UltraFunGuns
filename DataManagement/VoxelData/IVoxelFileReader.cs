﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UltraFunGuns
{
    public interface IVoxelFileReader
    {
        public VoxelWorldFile ReadWorldData(BinaryReader br);
        public VoxelWorldFileHeader ReadHeader(BinaryReader br);
        public SerializedVoxel ReadVoxel(BinaryReader br);
    }
}
