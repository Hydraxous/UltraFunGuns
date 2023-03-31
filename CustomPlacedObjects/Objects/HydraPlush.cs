﻿using Logic;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.CustomPlacedObjects.Objects
{
    public class HydraPlush : ICustomPlacedObject
    {
        public HydraPlush() { }

        public string[] GetScenePlacementNames()
        {
            return new string[] { "uk_construct", "CreditsMuseum2" }; 
        }

        public void Place(string sceneName)
        {
            switch(sceneName)
            {
                case "uk_construct":
                    Place(new Vector3(131.5651f, 13.5f, 622.0618f), Vector3.zero);
                    break;
                case "CreditsMuseum2":
                    //Place(new Vector3(75.01125f, 22.00494f, 742.4724f), Vector3.zero);
                    PlaceInMuseum();
                    break;
                default:
                    break;
            }
        }

        private void Place(Vector3 position, Vector3 rotation)
        {
            if (Prefabs.HydraPlushie == null)
                return;

            StaticCoroutine.DelayedExecute(() => { GameObject.Instantiate(Prefabs.HydraPlushie, position, Quaternion.Euler(rotation)); }, 10.0f);
        }

        private void PlaceInMuseum()
        {
            Place(new Vector3(-261.0277f, 75.00449f, 708.2495f), Vector3.zero);
            Place(new Vector3(75.01125f, 22.00494f, 742.4724f), Vector3.zero);
            Place(new Vector3(231.2761f, 75.04001f, 710.4579f), Vector3.zero);
        }

        private void PlaceInSandbox()
        {

        }
    }
}