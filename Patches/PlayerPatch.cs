using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

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

        public static void PostFix(NewMovement __instance)
        {
            if(!PlayerDead && __instance.dead)
            {
                PlayerDead = true;
                Events.OnPlayerDeath?.Invoke();
            }
        }
    }
}
