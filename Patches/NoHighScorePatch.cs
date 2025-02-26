using HarmonyLib;
using UnityEngine.SceneManagement;

namespace UltraFunGuns.Patches
{
    public static class NoHighScorePatch
    {
        [HarmonyPatch(typeof(LeaderboardController), nameof(LeaderboardController.SubmitCyberGrindScore)), HarmonyPrefix]
        public static bool OnSubmitCyberGrindScore()
        {
            UltraFunGuns.Log.LogWarning("Trying To Submit Cybergrind Highscore.");

            if (weaponsUsed)
            {
                UltraFunGuns.Log.LogWarning($"Modded weapons used. Disqualifying Cybergrind submission. WeaponsUsed:{weaponsUsed}");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(LeaderboardController), nameof(LeaderboardController.SubmitLevelScore)), HarmonyPrefix]
        public static bool OnSubmitLevelScore()
        {
            UltraFunGuns.Log.LogWarning($"Trying To Submit Level Highscore. WeaponsUsed:{weaponsUsed}");

            if (weaponsUsed)
            {
                UltraFunGuns.Log.LogWarning("Modded weapons used. Disqualifying Level submission.");
                return false;
            }
            return true;
        }


        public static void Init()
        {
            WeaponManager.OnWeaponsDeployed += CheckWeaponUsage;
            SceneManager.sceneLoaded += (_, __) => { weaponsUsed = false; };
        }

        private static bool weaponsUsed;
        public static void CheckWeaponUsage(UFGWeapon[] weaponsDeployed)
        {
            bool newWeaponUseState = weaponsDeployed.Length > 0;

            if (InGameCheck.CurrentLevelType == InGameCheck.UKLevelType.Endless)
            {
                if (weaponsUsed)
                {
                    HudMessageReceiver.Instance.SendHudMessage("You must restart the level after disabling UFG weapons for your score to count.");
                }
                else if (newWeaponUseState)
                {
                    HudMessageReceiver.Instance.SendHudMessage("Warning: having any UFG weapons enabled will prevent your CyberGrind score from being submitted.");
                }
            }

            if (newWeaponUseState)
            {
                weaponsUsed = true;
            }
        }
    }
}