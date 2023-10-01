using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [Serializable]
    public class VoxelGlass : IVoxelState
    {
        string testString = "defaultString";
        string  onInteractChangeTo = "Changed String!";

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("test", testString);
        }

        public VoxelGlass()
        {

        }

        protected VoxelGlass(SerializationInfo info, StreamingContext context)
        {
            testString = info.GetString("test");
        }

        public object GetStateData()
        {
            return this;
        }


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
            voxel.gameObject.AddComponent<SimpleBreakable>().OnBreak += voxel.Break;
        }

        public void OnVoxelUpdate(Voxel voxel)
        {

        }

        public void PrintState(Voxel voxel)
        {
            Debug.Log(testString);
        }

        public void SetStateData(object stateData)
        {
            VoxelGlass vglass = (VoxelGlass)stateData;
            testString = vglass.testString;
            Debug.LogWarning($"STATE DATA SET ON GLASS: ({testString})");
            //No clue if this works at all.
        }

        public void OnVoxelBreak(Voxel voxel)
        {
            Vector3 pos = voxel.transform.position;
            Debug.LogWarning("Glass broken!");
        }
    }
}
