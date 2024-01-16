using System;

namespace UltraFunGuns
{
    public static class VoxelFileReaderFactory
    {
        private static readonly VoxelFileReaderV0 readerV0 = new VoxelFileReaderV0();

        public static IVoxelFileReader GetReader(int version)
        {
            switch (version)
            {
                case 0:
                    return readerV0;
                default:
                    throw new Exception($"No voxel file reader found for version {version}, Is UltraFunGuns out of date? Current: ({ConstInfo.VERSION})");
            }
        }
    }
}
