﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using UltraFunGuns.Datas;
using UnityEngine;
using HydraDynamics.Keybinds;
using HydraDynamics;
using BepInEx;
using HydraDynamics.DataPersistence;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using HydraDynamics.Events;
using UltraFunGuns.Util;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using JetBrains.Annotations;

namespace UltraFunGuns
{
    public class DebuggingDummy : MonoBehaviour
    {
        private void Awake()
        {

        }

        private void Update()
        {

            if (!UltraFunGuns.DebugMode)
                return;

            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                PlayerDeathPatch.god = !PlayerDeathPatch.god;
                if (PlayerDeathPatch.god)
                {
                    SonicReverberator.vineBoom_Loud.PlayAudioClip();
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                Vector3 playerPos = NewMovement.Instance.transform.position;
                Vector3 cameraPos = CameraController.Instance.transform.position;

                Vector3 hitPosition = Vector3.zero;

                if (HydraUtils.SphereCastMacro(cameraPos, 0.001f, CameraController.Instance.transform.forward, Mathf.Infinity, out RaycastHit hit))
                {
                    hitPosition = hit.point;
                    Visualizer.DrawSphere(hitPosition, 0.25f, 5.0f);
                    Visualizer.DrawLine(5.0f, cameraPos, hitPosition);
                }


                string message =
                    $"Player: {playerPos.x}|{playerPos.y}|{playerPos.z}\n" +
                    $"Camera: {cameraPos.x}|{cameraPos.y}|{cameraPos.z}\n" +
                    $"HitPos: {hitPosition.x}|{hitPosition.y}|{hitPosition.z}";

                HydraLogger.Log(message, DebugChannel.Warning);
            }

            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                var allLocations = new List<IResourceLocation>();
                foreach (var resourceLocator in Addressables.ResourceLocators)
                {
                    if (resourceLocator is ResourceLocationMap map)
                    {
                        foreach (var locations in map.Locations.Values)
                        {
                            allLocations.AddRange(locations);
                        }
                    }
                }

                string data = "";

                foreach (IResourceLocation rlocation in allLocations)
                {
                    data += FormatResourceLocation(rlocation);
                }


                System.IO.File.WriteAllText("Z:/QuickThings/iodump/rlocation.txt", data);
            }
        }

        private string FormatResourceLocation(IResourceLocation rlocation)
        {
            string msg = "=================================\n";
            msg += "PKEY: " + rlocation.PrimaryKey + "\n";
            msg += "INTID: " + rlocation.InternalId + "\n";
            msg += "RTYPE: " + rlocation.ResourceType.Name + "\n";
            return msg;
        }

    }
}
