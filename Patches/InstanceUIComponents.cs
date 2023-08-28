using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Patches
{
    [HarmonyPatch(typeof(CanvasController))]
    public static class InstanceUIComponents
    {
        [HarmonyPatch("Awake"), HarmonyPostfix]
        public static void SpawnUI(CanvasController __instance)
        {
            RectTransform rt = __instance.GetComponent<RectTransform>();

            GameObject configMenuPrefab = HydraLoader.LoadAssetOfType<GameObject>("ConfigurationMenu");
            GameObject.Instantiate(configMenuPrefab, rt);

        }
    }
}
