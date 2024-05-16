using UnityEngine;

namespace UltraFunGuns
{
    public class MySphere : IVoxelStructure
    {
        private const string fillID = "cobblestone";
        private VoxelData fillVoxel;

        public void Build(Vector3Int origin, int seed)
        {
            fillVoxel = VoxelDatabase.GetVoxelData(fillID);
            int radius = new System.Random(seed).Next(2, 10);
            BuildSphere(origin, radius);
        }

        private void BuildSphere(Vector3Int origin, int rad)
        {
            Vector3Int min = origin - new Vector3Int(rad, rad, rad);
            Vector3Int max = origin + new Vector3Int(rad, rad, rad);

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);
                        if (Vector3Int.Distance(pos, origin) <= rad)
                        {
                            Voxel.Build(VoxelLocation.CoordinateToLocation(new Vector3Int(x,y,z)), fillVoxel);
                        }
                    }
                }
            }
        }
    }
}
