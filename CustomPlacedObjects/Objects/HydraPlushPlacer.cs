﻿using UnityEngine;

namespace UltraFunGuns.CustomPlacedObjects.Objects
{
    public class HydraPlushPlacer : ICustomPlacedObject
    {
        public HydraPlushPlacer() { }

        public string[] GetScenePlacementNames()
        {
            return new string[] { "uk_construct", "CreditsMuseum2" }; 
        }

        public bool Place(string sceneName)
        {
            if (sceneName != "CreditsMuseum2")
                return false;

            //Place(new Vector3(-261.0277f, 75.00449f, 708.2495f), Vector3.zero);
            //Place(new Vector3(75.01125f, 22.00494f, 742.4724f), Vector3.zero);
            Place(new Vector3(231.2761f, 75.04001f, 710.4579f), new Vector3(0, 90.0f, 0.0f));
            return true;
        }

        private void Place(Vector3 position, Vector3 rotation)
        {
            if (Prefabs.HydraPlushie == null)
                return;

            StaticCoroutine.DelayedExecute(() => { GameObject.Instantiate(Prefabs.HydraPlushie, position, Quaternion.Euler(rotation)); }, 10.0f);
        }
    }
}
