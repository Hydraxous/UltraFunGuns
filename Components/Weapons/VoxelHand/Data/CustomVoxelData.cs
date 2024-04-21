using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [Serializable]
    public class CustomVoxelData : VoxelData
    {
        public string BehaviourStateID { get; }
        public string MeshOverrideID { get; }
        public string AudioPath { get; }
        public string TexturePath { get; }

        public CustomVoxelData(string iD, string displayName, Material material, AudioClip sound, string stateID, string texturePath, string audioPath, string meshOverrideID) : base(iD, displayName, material, sound)
        {
            BehaviourStateID = stateID;
            TexturePath = texturePath;
            AudioPath = audioPath;
            MeshOverrideID = meshOverrideID;
        }
    }
}
