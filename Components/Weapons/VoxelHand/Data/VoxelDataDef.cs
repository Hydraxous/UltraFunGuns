using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class VoxelDataDef : ScriptableObject
    {
        //Due to Unity's *odd* serialization. We have to use ScriptableObject to serialize custom types.

        //Fields here match the properties of VoxelData
        public string DisplayName;
        public Texture2D Texture;
        public Material MaterialOverride;
        public AudioClip Sound;

        private VoxelData _voxelData;

        public VoxelData VoxelData
        {
            get
            {
                if(_voxelData == null)
                {
                    _voxelData = ConvertData();
                }

                return _voxelData;
            }
        }

        //Converted to the actual type at runtime.
        private VoxelData ConvertData()
        {
            return new VoxelData(name, DisplayName, (MaterialOverride == null) ? VoxelMaterialManager.GetMaterial(Texture) : MaterialOverride, Sound);
        }
    }
}
