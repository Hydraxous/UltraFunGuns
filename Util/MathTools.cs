using System;
using System.Collections.Generic;
using System.Text;
using UltraFunGuns.Datas;
using UnityEngine;

namespace UltraFunGuns
{
    public static class MathTools
    {

        public static Vector3 Abs(this Vector3 vector)
        {
            vector.x = Mathf.Abs(vector.x);
            vector.y = Mathf.Abs(vector.y);
            vector.z = Mathf.Abs(vector.z);
            return vector;
        }

        public static Vector3 CalculateProjectileArcPosition(Vector3 start, Vector3 end, float airTime, float normalizedTime)
        {
            Vector3 initialDirection = GetVelocityTrajectory(start, end, airTime);

            Vector3[] trajectoryPoints = GetTrajectoryPoints(start, Physics.gravity, initialDirection, 0.0f, airTime);

            int index = Mathf.FloorToInt(trajectoryPoints.Length - 1 * normalizedTime);

            return trajectoryPoints[index];
        }

        public static bool ConeCheck(Vector3 direction1, Vector3 direction2, float maximumAngle = 90.0f)
        {
            return Vector3.Angle(direction1, direction2) <= maximumAngle;
        }

        /// <summary>
        /// Get a velocity vector for a projectile parabola with a specified flight time.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End Position</param>
        /// <param name="airTime">Total flight time</param>
        /// <returns></returns>
        public static Vector3 GetVelocityTrajectory(Vector3 start, Vector3 end, float airTime)
        {
            Vector3 gravity = Physics.gravity;

            airTime = (airTime == 0) ? airTime + 0.0001f : airTime;

            Vector3 parabolaMiddleHeight = Vector3.LerpUnclamped(start, end, 0.5f / airTime);
            parabolaMiddleHeight -= gravity * airTime;

            Vector3 shootDirection = parabolaMiddleHeight - start;

            return shootDirection;
        }

        /// <summary>
        /// Returns local quaternion rotation based off of world space quaternion
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="worldRotation"></param>
        /// <returns></returns>
        public static Quaternion WorldToLocalRotation(this Transform transform, Quaternion worldRotation)
        {
            return Quaternion.Inverse(transform.rotation) * worldRotation;
        }

        //TODO optimize this. This does not do what it was ported for. Purpose is for getting a point along a trajectory at a given time. This function is for drawing said trajectory, not querying it.
        private static Vector3[] GetTrajectoryPoints(Vector3 start, Vector3 gravity, Vector3 direction, float drag, float flightTime)
        {

            Vector3 currentVelocity = direction;

            float positionStep = flightTime / 100;

            Vector3 lastPosition = start;

            List<Vector3> points = new List<Vector3>();

            for (int i = 0; i < 100 * 2; i++)
            {
                currentVelocity = currentVelocity * (1 - positionStep * drag);

                currentVelocity += gravity * positionStep;

                points.Add(lastPosition);

                lastPosition += currentVelocity * positionStep;
            }

            return points.ToArray();
        }

        public static void Cycle(ref this int i, int amount, int max)
        {
            if(max <= 1)
            {
                i = 0;
                return;
            }

            i += amount;

            while(i >= max)
            {
                i -= max;
            }

            while(i < 0)
            {
                i = max - i;
            }
        }

    }
}
