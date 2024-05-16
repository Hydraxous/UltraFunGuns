using System;
using System.IO;
using UnityEngine;

namespace UltraFunGuns
{
    [Serializable]
    public class VoxelGlass : IVoxelState
    {

        public void OnVoxel(Voxel voxel)
        {

        }

        public void OnVoxelDestroyed(Voxel voxel)
        {

        }


        public void OnVoxelInteract(Voxel voxel)
        {
            PrintState(voxel);
        }

        public void OnVoxelPlaced(Voxel voxel)
        {
            voxel.gameObject.AddComponent<Explodable>().OnExplode += (e) =>
            {
                if(e == null)
                    if (e.harmless)
                        return;

                voxel.Break();
            };

            voxel.gameObject.AddComponent<SimpleBreakable>().OnBreak += voxel.Break;
        }

        public void OnVoxelUpdate(Voxel voxel)
        {

        }

        public void PrintState(Voxel voxel) {}

        public void SetStateData(object stateData) {}

        public void OnVoxelBreak(Voxel voxel)
        {
            Vector3 pos = voxel.transform.position;
            Debug.LogWarning("Glass broken!");
        }

        public BinaryWriter WriteStateData(BinaryWriter bw)
        {
            return bw;
        }

        public void ReadStateData(BinaryReader br)
        {
            //Nothing to read :)
        }

        public string GetID()
        {
            return "glass";
        }
    }
}
