﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Patches
{
    [HarmonyPatch(typeof(CanvasController))]
    public static class InstanceUIComponents
    {
        [UFGAsset("VoxelHandOverlay")]
        private static GameObject voxelHandOverlay;

        public static RectTransform Rect { get; private set; }

        [HarmonyPatch("Awake"), HarmonyPostfix]
        public static void SpawnUI(CanvasController __instance)
        {
            Rect = __instance.GetComponent<RectTransform>();
            RectTransform rt = __instance.GetComponent<RectTransform>();

            GameObject voxelHandOverlayInstance = GameObject.Instantiate<GameObject>(voxelHandOverlay, rt);
        }
    }
}
