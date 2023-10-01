using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace UltraFunGuns
{
    public interface IVoxelState : ISerializable
    {
        //Called when Deserialized.
        public void SetStateData(object stateData);

        //Called for serialization
        public object GetStateData();

        //Called when the state is set on a voxel
        public void OnVoxel(Voxel voxel);

        //Called in Voxel.Start()
        public void OnVoxelPlaced(Voxel voxel);

        //Called in Voxel.OnDestroy
        public void OnVoxelDestroyed(Voxel voxel);

        //Called by Voxel.VoxelUpdate
        public void OnVoxelUpdate(Voxel voxel);

        //Called by Voxel.Interact
        public void OnVoxelInteract(Voxel voxel);

        //Called by Voxel.Break
        public void OnVoxelBreak(Voxel voxel);

        public void PrintState(Voxel voxel);
    }
}
