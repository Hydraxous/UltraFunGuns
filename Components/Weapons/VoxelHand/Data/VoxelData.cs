using Newtonsoft.Json;
using UnityEngine;

namespace UltraFunGuns
{
    public class VoxelData
    {
        public string ID { get; }
        public string DisplayName { get; }

        [JsonIgnore]
        public Material Material { get; }

        [JsonIgnore]
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
