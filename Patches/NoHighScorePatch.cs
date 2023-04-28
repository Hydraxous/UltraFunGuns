using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltraFunGuns.Patches
{
    public class CheatsPatch
    {
        [HarmonyPatch(typeof(LeaderboardController))]
        public static class NoHighScorePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("SubmitCyberGrindScore")]
            public static bool CyberGrindPrefix()
            {
                HydraLogger.Log("Trying To Submit Cybergrind Highscore.", DebugChannel.Warning);

                if (weaponsUsed)
                {
                    HydraLogger.Log($"Modded weapons used. Disqualifying Cybergrind submission. WeaponsUsed:{weaponsUsed}", DebugChannel.Warning);
                    return false;
                }
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("SubmitLevelScore")]
            public static bool LevelScorePrefix()
            {
                HydraLogger.Log($"Trying To Submit Level Highscore. WeaponsUsed:{weaponsUsed}", DebugChannel.Warning);

                if (weaponsUsed)
                {
                    HydraLogger.Log("Modded weapons used. Disqualifying Level submission.", DebugChannel.Warning);
                    return false;
                }
                return true;
            }


            public static void Init()
            {
                WeaponManager.OnWeaponsDeployed += CheckWeaponUsage;
                SceneManager.sceneLoaded += (_,__) => { weaponsUsed = false; };
            }

            private static bool weaponsUsed;
            public static void CheckWeaponUsage(UFGWeapon[] weaponsDeployed)
            {
                bool newWeaponUseState = weaponsDeployed.Length > 0;

                if(InGameCheck.CurrentLevelType == InGameCheck.UKLevelType.Endless)
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

                if(newWeaponUseState)
                {
                    weaponsUsed = true;
                }
            }
        }
    }
}