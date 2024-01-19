using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class VoxelData
    {
        public string ID { get; }
        public string DisplayName { get; }
        public Material Material { get; }
        public AudioClip Sound { get; }

        public VoxelData(string iD, string displayName, Material material, AudioClip sound)
        {
            ID = iD;
            DisplayName = displayName;
            Material = material;
            Sound = sound;
        }
    }
}
