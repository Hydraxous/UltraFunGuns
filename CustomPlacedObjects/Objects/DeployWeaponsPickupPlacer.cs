using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.CustomPlacedObjects.Objects
{
    public class DeployWeaponsPickupPlacer : ICustomPlacedObject
    {
        public string[] GetScenePlacementNames()
        {
            return new string[] { "Level 5-S" };
        }

        public bool Place(string sceneName)
        {
            switch(sceneName)
            {
                case "Level 5-S":
                    Place(new Vector3(43.9f, -39.7f, 34.20f), new Vector3(7.14f, 0.0f, 0.0f));
                    return true;
            }

            return false;
        }

        private void Place(Vector3 position, Vector3 rotation)
        {
            if (Prefabs.ForceDeployPickup == null)
                return;

            GameObject.Instantiate(Prefabs.ForceDeployPickup, position, Quaternion.Euler(rotation));
        }
    }
}
