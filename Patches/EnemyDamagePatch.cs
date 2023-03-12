using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(EnemyIdentifier), "DeliverDamage")]
    public static class EnemyDamagePatch
    {
        private static Dictionary<EnemyIdentifier, float> healthCache = new Dictionary<EnemyIdentifier, float>();

        public static bool Prefix(EnemyIdentifier __instance)
        {
            if(__instance.dead || !Data.Config.Data.DebugMode)
            {
                return true;
            }

            if(!healthCache.ContainsKey(__instance))
            {
                healthCache.Add(__instance, __instance.health);
            }

            return true;
        }

        public static void Postfix(EnemyIdentifier __instance, Vector3 force, Vector3 hitPoint, float multiplier, bool tryForExplode, float critMultiplier)
        {
            if(Data.Config.Data.DebugMode)
            {
                float damageNum = 0;

                if(healthCache.ContainsKey(__instance))
                {
                    damageNum = healthCache[__instance] - __instance.health;
                    healthCache.Remove(__instance);
                }
                string msg = $"\nFORCE: {force.x}|{force.y}|{force.z}|({force.magnitude})\nMUL: {multiplier}\nCRIT: {critMultiplier}\nDMG: {damageNum}";
                HydraLogger.Log(msg);
                Visualizer.DisplayTextAtPosition(msg, hitPoint, Color.red,10.0f);
            }
        }
    }
}
