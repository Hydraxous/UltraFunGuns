using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    [Serializable]
    public struct SerializedVoxel : ISerializable
    {
        public int x;
        public int y;
        public int z;
        public string id;
        public Type stateType;
        public object stateData;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("x", x);
            info.AddValue("y", y);
            info.AddValue("z", z);
            info.AddValue("id", id);
            info.AddValue("type", (stateType == null) ? "" : stateType.FullName);
            info.AddValue("state", stateData);
        }

        public SerializedVoxel(int x, int y, int z, string id, IVoxelState state)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.id = id;

            this.stateData = state?.GetStateData();
            this.stateType = state?.GetType();
        }

        private SerializedVoxel(SerializationInfo info, StreamingContext context)
        {
            x = info.GetInt32("x");
            y = info.GetInt32("y");
            z = info.GetInt32("z");
            id = info.GetString("id");
            stateType = Type.GetType(info.GetString("type"));
            
            if (stateType != null)
                stateData = info.GetValue("state", stateType);
        }
    }
}
