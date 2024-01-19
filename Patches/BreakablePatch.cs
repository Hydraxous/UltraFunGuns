using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns.Patches
{
    public static class BreakablePatch 
    {
        [HarmonyPatch(typeof(Breakable), "Start"), HarmonyPostfix]
        public static void FixBreakable(Breakable __instance)
        {
            __instance.gameObject.AddComponent<SimpleBreakable>().OnBreak += __instance.Break;
        }

        [HarmonyPatch(typeof(Glass)), HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor)]
        public static void FixGlass(Glass __instance)
        {
            __instance.gameObject.AddComponent<SimpleBreakable>().OnBreak += __instance.Shatter;
        }
    }
}
