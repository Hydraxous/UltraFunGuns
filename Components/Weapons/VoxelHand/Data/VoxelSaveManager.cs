﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public static class VoxelSaveManager
    {
        public static IEnumerable<VoxelWorldFile> LoadWorldDatas()
        {
            if (!Directory.Exists(Paths.VoxelSavesFolder))
                yield break;

            foreach (var filePath in Directory.GetFiles(Paths.VoxelSavesFolder, '*' + Paths.VOXEL_SAVE_FILE_EXTENSION, SearchOption.AllDirectories))
            {
                if (!File.Exists(filePath))
                    continue;

                VoxelWorldFile worldData = null;

                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    worldData = (VoxelWorldFile)binaryFormatter.Deserialize(file);
                }

                yield return worldData;
            }
        }

        public static IEnumerable<VoxelWorldFileHeader> LoadHeaders()
        {
            if (!Directory.Exists(Paths.VoxelSavesFolder))
                yield break;

            foreach (var filePath in Directory.GetFiles(Paths.VoxelSavesFolder, '*' + Paths.VOXEL_SAVE_FILE_EXTENSION, SearchOption.AllDirectories))
            {
                if (!File.Exists(filePath))
                    continue;

                yield return LoadHeaderAtFilePath(filePath);
            }
        }

        public static VoxelWorldFileHeader LoadHeaderAtFilePath(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            VoxelWorldFileHeader header = null;
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(file))
                {
                    int fileVersion = br.ReadInt32();
                    IVoxelFileReader fileInterpreter = VoxelFileReaderFactory.GetReader(fileVersion);
                    header = fileInterpreter.ReadHeader(br);
                    header.FilePath = filePath;
                }
            }

            return header;
        }

        public static VoxelWorldFile LoadAtFilePath(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            VoxelWorldFile worldData = null;

            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(file))
                {
                    int fileVersion = br.ReadInt32();
                    IVoxelFileReader fileInterpreter = VoxelFileReaderFactory.GetReader(fileVersion);
                    worldData = fileInterpreter.ReadWorldData(br);
                    worldData.Header.FilePath = filePath;
                }
            }

            return worldData;
        }

        public static byte[] ToBytes(VoxelWorldFile file)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(VoxelWorldFile.FILE_VERSION);
                    file.WriteBytes(bw);
                }
                return ms.ToArray();
            }
        }

        public static VoxelWorldFile LoadFromName(string name)
        {
            string filePath = Path.Combine(Paths.VoxelSavesFolder, name+Paths.VOXEL_SAVE_FILE_EXTENSION);
            return LoadAtFilePath(filePath);
        }

        public static bool Exists(string name)
        {
            string filePath = NameToFilePath(name);
            return File.Exists(filePath);
        }

        public static void SaveWorldData(string fileName, VoxelWorldFile data)
        {
            if(!Directory.Exists(Paths.VoxelSavesFolder))
                Directory.CreateDirectory(Paths.VoxelSavesFolder);

            string filePath = NameToFilePath(fileName);
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter bw = new BinaryWriter(file))
                {
                    bw.Write(VoxelWorldFile.FILE_VERSION);
                    data.WriteBytes(bw);
                }
            }
        }

        public static string NameToFilePath(string name)
        {
            return Path.Combine(Paths.VoxelSavesFolder, name + Paths.VOXEL_SAVE_FILE_EXTENSION);
        }
    }
}
