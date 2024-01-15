using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.CustomPlacedObjects.Objects
{
    public class BasketballHoopPlacer : ICustomPlacedObject
    {
        [UFGAsset("BasketballHoop")] private static GameObject basketBallHoop;

        public string[] GetScenePlacementNames()
        {
            return new string[] { "uk_construct" };
        }

        public bool Place(string sceneName)
        {
            if (sceneName != "uk_construct" || basketBallHoop == null)
                return false;

            GameObject.Instantiate(basketBallHoop, new Vector3(-136.0f,-1.5f,362.2f), Quaternion.identity);
            return true;
        }
    }
}
