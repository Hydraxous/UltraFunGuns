using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public interface IVoxelStructure
    {
        public void Build(Vector3Int origin, int seed);
    }
}
