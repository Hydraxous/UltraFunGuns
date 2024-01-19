using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public class MyVoxelHouse : IVoxelStructure
    {

        private Vector3Int origin;
        private int seed;

        private const string floorID = "cobblestone";
        private const string wallID = "wooden_planks";
        private const string pillarID = "log";
        private const string glassID = "glass";
        private const string floorBorderID = "smooth_stone";

        private int height;
        private int width;
        private int length;

        private VoxelData floorVoxel;
        private VoxelData wallVoxel;
        private VoxelData glassVoxel;
        private VoxelData pillarVoxel;
        private VoxelData floorBorderVoxel;

        private const int maxHeight = 9;
        private const int kneeHeight = 1;

        private const int heightSize = 4;
        private const int widthSize = 4;
        private const int lengthSize = 4;

        public void Build(Vector3Int origin, int seed)
        {
            this.origin = origin;
            this.seed = seed;

            floorVoxel = VoxelDatabase.GetVoxelData(floorID);
            wallVoxel = VoxelDatabase.GetVoxelData(wallID);
            pillarVoxel = VoxelDatabase.GetVoxelData(pillarID);
            glassVoxel = VoxelDatabase.GetVoxelData(glassID);
            floorBorderVoxel = VoxelDatabase.GetVoxelData(floorBorderID);

            System.Random rand = new System.Random(seed);
            height = rand.Next(maxHeight);
            width = rand.Next(maxHeight);
            length = rand.Next(maxHeight);

            BuildBuilding(length, width, height);
        }


        private void BuildBuilding(int length, int width, int height)
        {
            int totalHeight = (height * heightSize)+1;

            int totalWidth = (width * widthSize)+1;
            int totalLength = (length * lengthSize)+1;

            for(int i=0;i<totalHeight;i++)
            {
                bool floorLayer = i % heightSize == 0;
                if (floorLayer)
                {
                    BuildFloorLayer(i, totalLength, totalWidth);
                    continue;
                }

                bool kneeLayer = (i-1) % heightSize == 0;
                VoxelData wallMaterial = (kneeLayer) ? wallVoxel : glassVoxel;

                BuildWalls(i, totalLength, totalWidth, wallMaterial, pillarVoxel);
            }
        }

        private void BuildFloorLayer(int y, int length, int width)
        {
            VoxelLocation currentLocation = new VoxelLocation();

            for (int x = 0; x < length; x++)
            {
                for(int z = 0; z < width; z++)
                {
                    bool isEdge = (x == 0 || x == length-1) || (z == 0 || z == width-1);

                    currentLocation.Coordinate = origin + new Vector3Int(x, y, z);
                    VoxelData currentVoxel = (isEdge) ? floorBorderVoxel : floorVoxel;
                    Voxel.Build(currentLocation, currentVoxel);
                }
            }
        }

       

        private void BuildWalls(int y, int length, int width, VoxelData wallMaterial, VoxelData pillarMaterial)
        {
            VoxelLocation currentLocation = new VoxelLocation();

            for (int x = 0; x < length; x++)
            {
                for (int z = 0; z < width; z++)
                {
                    bool supportArea = (z % widthSize == 0 && x % lengthSize == 0);
                    bool isEdge = (x == 0 || x == length - 1) || (z == 0 || z == width - 1);

                    if (!supportArea && !isEdge)
                        continue;

                    VoxelData currentVoxel = (supportArea) ? pillarMaterial : wallMaterial; 
                    currentLocation.Coordinate = origin + new Vector3Int(x, y, z);
                    Voxel.Build(currentLocation, currentVoxel);
                }
            }
        }


        private void BuildLayer(int y, int length, int width, Action<int, int> onSpot)
        {
            for (int x = 0; x < length; x++)
            {
                for (int z = 0; z < width; z++)
                {
                    onSpot?.Invoke(x,z);
                }
            }
        }
    }
}
