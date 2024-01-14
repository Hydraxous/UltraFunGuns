using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UltraFunGuns.Components;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(Magnet))]
    public static class MagnetTest
    {

        [HarmonyPatch(typeof(Magnet), "Start"), HarmonyPostfix]
        public static void OnStart(Magnet __instance)
        {
            __instance.gameObject.AddComponent<CoinTarget>();
        }
    }
}
