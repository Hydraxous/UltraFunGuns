using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    //Coordinate is its logical integer coordinate in the VoxelWorld data structure.
    //Position is its world space Unity transform position.

    public struct VoxelLocation
    {
        private Vector3Int _coordinate;
        private Vector3 _position;

        public VoxelLocation(Vector3Int coordinate)
        {
            Coordinate = coordinate;
        }

        public VoxelLocation(Vector3 position)
        {
            Position = position;
        }

        public Vector3Int Coordinate
        {
            get
            {
                return _coordinate;
            }

            set
            {
                _coordinate = value;
                _position = CoordinateToPosition(value);
            }
        }

        public Vector3 Position
        {
            get
            {
                return _position;
            }

            set
            {
                _position = value;
                _coordinate = PositionToCoordinate(value);
            }
        }

        public static VoxelLocation CoordinateToLocation(Vector3Int coordinate)
        {
            return new VoxelLocation(coordinate);
        }

        public static VoxelLocation PositionToLocation(Vector3 position)
        {
            return new VoxelLocation(position);
        }

        public static Vector3 CoordinateToPosition(Vector3Int coordinate)
        {
            return new Vector3(coordinate.x, coordinate.y, coordinate.z) * VoxelWorld.WorldScale;
        }

        public static Vector3Int PositionToCoordinate(Vector3 position)
        {
            Vector3 snappedPosition = SnapToWorldGrid(position);
            return new Vector3Int()
            {
                x = (int)(snappedPosition.x / VoxelWorld.WorldScale),
                y = (int)(snappedPosition.y / VoxelWorld.WorldScale),
                z = (int)(snappedPosition.z / VoxelWorld.WorldScale),
            };
        }

        public static Vector3 SnapToWorldGrid(Vector3 samplePosition)
        {
            samplePosition.x = SnapAxis(samplePosition.x, VoxelWorld.WorldScale);
            samplePosition.y = SnapAxis(samplePosition.y, VoxelWorld.WorldScale);
            samplePosition.z = SnapAxis(samplePosition.z, VoxelWorld.WorldScale);
            return samplePosition;
        }

        //Yucky but it works
        public static float SnapAxis(float value, float interval)
        {
            float valueSign = Mathf.Sign(value);
            value = Mathf.Abs(value);
            float remainder = value % interval;

            if (remainder > interval / 2f)
                value = (value - remainder) + interval;
            else
                value = value - remainder;

            return value * valueSign;
        }
    }
}
