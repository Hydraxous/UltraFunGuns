using System.Collections.Generic;
using UnityEngine;

namespace UltraFunGuns
{
    public static class Visualizer
    {
        [UFGAsset("DebugSphere")] private static GameObject debugSphere;
        [UFGAsset("DebugCube")] private static GameObject debugCube;
        [UFGAsset("DebugLine")]private static GameObject debugLine;

        private static bool visualizerEnabled => (UltraFunGuns.DebugMode && Data.Config.Data.EnableVisualizer);

        [Commands.UFGDebugMethod("ToggleVisualizer", "Toggles Visual Debugging")]
        public static void ToggleVisualizer()
        {
            bool nowEnabled = !Data.Config.Data.EnableVisualizer;
            Data.Config.Data.EnableVisualizer = nowEnabled;
            Data.Config.Save();

            HydraLogger.Log($"Visual Debugging: {nowEnabled}", DebugChannel.User);
        }

        public static void DrawSphere(Vector3 position, float radius, float time = 1.0f)
        {
            if(!visualizerEnabled)
            {
                return;
            }

            if (debugSphere == null)
            {
                return;
            }

            Transform newDebugSphere = GameObject.Instantiate<GameObject>(debugSphere, position, Quaternion.identity).transform;
            newDebugSphere.localScale *= radius;
            newDebugSphere.gameObject.AddComponent<DestroyAfterTime>().TimeLeft = time;
        }

        public static void DrawLine(float time, params Vector3[] points)
        {
            if (!visualizerEnabled)
            {
                return;
            }

            if (debugLine == null)
            {
                return;
            }

            if (points.Length < 2)
            {
                HydraLogger.Log("Not enough points provided for Visualizer.DrawLine", DebugChannel.Error);
                return;
            }

            DebugLine newDebugLine = GameObject.Instantiate<GameObject>(debugLine, points[0], Quaternion.identity).GetComponent<DebugLine>();
            newDebugLine.SetLine(points);
            DestroyAfterTime destroyAfterTime = newDebugLine.gameObject.AddComponent<DestroyAfterTime>();
            destroyAfterTime.TimeLeft = time;
        }

        public static void DrawSphereCast(Vector3 origin, float radius, Vector3 direction, float maxRange, float time)
        {
            if (!UltraFunGuns.DebugMode || !Data.Config.Data.EnableVisualizer)
                return;
            
            if (radius > 0)
            {
                int sphereCount = Mathf.CeilToInt(maxRange / radius);

                Vector3 currentPos = origin;

                for(int i =0; i < sphereCount; i++)
                {
                    DrawSphere(currentPos, radius, time);
                    currentPos += direction.normalized * radius;
                }
            }
            DrawRay(origin, direction.normalized * maxRange, time);
        }

        public static void DrawRay(Vector3 position, Vector3 direction, float time = 0.4f)
        {
            if (!UltraFunGuns.DebugMode || !Data.Config.Data.EnableVisualizer)
            {
                return;
            }

            DrawLine(time, position, position + direction);
        }

        [UFGAsset("DebugTextPopup")] private static GameObject debugTextPopup;

        private static List<Vector3> currentPositions = new List<Vector3>();

        private static Dictionary<DebugTextPopup, Vector3> cachedTexts = new Dictionary<DebugTextPopup, Vector3>();

        public static void DisplayTextAtPosition(string text, Vector3 position, Color color, float killTime = 3.0f)
        {
            if (!visualizerEnabled)
            {
                return;
            }

            if (debugTextPopup != null)
            {
                while(currentPositions.Contains(position))
                {
                    position += UnityEngine.Random.onUnitSphere*2f;
                }

                currentPositions.Add(position);

                GameObject newText = GameObject.Instantiate<GameObject>(debugTextPopup, position, Quaternion.identity);
                DebugTextPopup textPopup = newText.GetComponent<DebugTextPopup>();
                cachedTexts.Add(textPopup, position);
                textPopup.SetText(text, color);
                textPopup.SetKillTime(killTime);
            }
        }

        public static void ClearDebugText(DebugTextPopup textPopup)
        {
            if(cachedTexts.ContainsKey(textPopup))
            {
                currentPositions.Remove(cachedTexts[textPopup]);
                cachedTexts.Remove(textPopup);
            }
        }
    }
}
