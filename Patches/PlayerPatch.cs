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
        public static void PostFix(NewMovement __instance, int damage)
        {
            Events.PlayerHurt(__instance, damage);

            if(!PlayerDead && (__instance.dead || __instance.hp <= 0))
            {
                PlayerDead = true;
                Events.OnPlayerDeath?.Invoke();
                UltraFunGuns.Log.LogWarning("Player died!");
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
