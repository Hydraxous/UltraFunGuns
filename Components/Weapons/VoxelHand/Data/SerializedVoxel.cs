using System;
using System.IO;

namespace UltraFunGuns
{
    [Serializable]
    public struct SerializedVoxel
    {
        public int x;
        public int y;
        public int z;
        public string id;
        public string stateTypeID;
        public byte[] stateData;

        public BinaryWriter WriteBytes(BinaryWriter bw)
        {
            bw.Write(x);
            bw.Write(y);
            bw.Write(z);
            bw.Write(id);
            bw.Write(stateTypeID);
            bw.Write(stateData.Length);
            bw.Write(stateData);
            return bw;
        }
    }
}
