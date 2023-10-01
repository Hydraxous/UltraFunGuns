using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class RunawayBlock : IVoxelState
    {
        public const string blockID = "Waystone.png";
        private int travels = 0;

        public RunawayBlock()
        {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("0", travels);
        }

        public object GetStateData()
        {
            return this;
        }

        public void OnVoxel(Voxel voxel)
        {

        }

        public void OnVoxelBreak(Voxel voxel)
        {

        }

        public void OnVoxelDestroyed(Voxel voxel)
        {

        }

        public void OnVoxelInteract(Voxel voxel)
        {
            VoxelLocation currentLocation = voxel.GetLocation();
            PrintState(voxel);
        }

        
        public void OnVoxelPlaced(Voxel voxel)
        {

        }

        public void OnVoxelUpdate(Voxel voxel)
        {

        }

        public void PrintState(Voxel voxel)
        {
            Debug.LogWarning($"Current travels: {travels}");
        }

        public void SetStateData(object stateData)
        {
            RunawayBlock rb = (RunawayBlock)stateData;
            travels = rb.travels;
        }
    }
}
