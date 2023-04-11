using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UltraFunGuns.Util;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(NewMovement), "Respawn")]
    public static class PlayerRespawnPatch
    {
        public static void PostFix()
        {
            Events.OnPlayerRespawn?.Invoke();
            PlayerDeathPatch.PlayerDead = false;
        }
    }

    [HarmonyPatch(typeof(NewMovement), "GetHurt")]
    public static class PlayerDeathPatch
    {
        public static bool PlayerDead;

        public static bool god;
        public static bool Prefix(NewMovement __instance, ref int damage)
        {
            if(!god)
            {
                return true;
            }

            if (__instance.hp - damage <= 0)
            {
                __instance.hp = 1;
                SonicReverberator.vineBoom_Loudest.PlayAudioClip();
                return false;
            }

            return true;
        }

        public static void PostFix(NewMovement __instance)
        {
            if(!PlayerDead && (__instance.dead || __instance.hp <= 0))
            {
                PlayerDead = true;
                Events.OnPlayerDeath?.Invoke();
                HydraLogger.Log("Player died!",DebugChannel.Warning);
            }
        }
    }


    [HarmonyPatch(typeof(StatsManager))]
    public static class StasManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(StatsManager.Restart))]
        public static void Postfix()
        {
            Cleaner.Clean();
        }
    }


    [HarmonyPatch(typeof(NewMovement))]
    public static class InputDisruptor
    {

    }
}
