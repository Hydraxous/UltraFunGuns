using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns
{
    public static class Visualizer
    {
        private static GameObject debugSphere;
        private static GameObject debugCube;
        private static GameObject debugLine;

        public static void Init()
        {
            HydraLoader.prefabRegistry.TryGetValue("DebugSphere", out debugSphere);
            HydraLoader.prefabRegistry.TryGetValue("DebugLine", out debugLine);
            HydraLoader.prefabRegistry.TryGetValue("DebugCube", out debugCube);
        }

        public static void DrawSphere(Vector3 position, float radius, float time = 1.0f)
        {
            if(!UltraFunGuns.DebugMode)
            {
                return;
            }

            if(debugSphere == null)
            {
                return;
            }

            Transform newDebugSphere = GameObject.Instantiate<GameObject>(debugSphere, position, Quaternion.identity).transform;
            newDebugSphere.localScale *= radius;
            newDebugSphere.gameObject.AddComponent<DestroyAfterTime>().TimeLeft = time;
        }

        public static void DrawLine(float time, params Vector3[] points)
        {
            if (!UltraFunGuns.DebugMode)
            {
                return;
            }
            
            if (debugLine == null)
            {
                return;
            }

            if(points.Length < 2)
            {
                HydraLogger.Log("Not enough points provided for Visualizer.DrawLine", DebugChannel.Error);
                return;
            }

            DebugLine newDebugLine = GameObject.Instantiate<GameObject>(debugLine, points[0], Quaternion.identity).GetComponent<DebugLine>();
            newDebugLine.SetLine(points);
            DestroyAfterTime destroyAfterTime = newDebugLine.gameObject.AddComponent<DestroyAfterTime>();
            destroyAfterTime.TimeLeft = time;
        }

        public static void DrawRay(Vector3 position, Vector3 direction, float time = 0.4f)
        {
            if (!UltraFunGuns.DebugMode)
            {
                return;
            }

            DrawLine(time, position, position + direction);
        }
    }
}
