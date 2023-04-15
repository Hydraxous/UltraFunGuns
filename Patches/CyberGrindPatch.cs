using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Patches
{
    public class CheatsPatch
    {
        [HarmonyPatch(typeof(SteamController), nameof(SteamController.SubmitCyberGrindScore))]
        public static class CyberGrindPreventer
        {
            public static bool Prefix(SteamController __instance)
            {
                if (weaponsUsed)
                {

                    HydraLogger.Log("Modded weapons used. Disqualifying Cybergrind submission.", DebugChannel.Warning);
                    return false;
                }

                return true;
            }


            public static void Init()
            {
                WeaponManager.OnWeaponsDeployed += CheckWeaponUsage;
            }

            private static bool weaponsUsed;
            public static void CheckWeaponUsage(UFGWeapon[] weaponsDeployed)
            {
                if (UKAPIP.CurrentLevelType != UKAPIP.UKLevelType.Endless)
                {
                    weaponsUsed = false;
                    return;
                }

                bool newWeaponUseState = weaponsDeployed.Length > 0;

                if (weaponsUsed)
                {
                    if (!EndlessGrid.Instance.GetComponent<Collider>().enabled)
                    {
                        HudMessageReceiver.Instance.SendHudMessage("You must restart the level after disabling UFG weapons for your score to count.");
                        weaponsUsed = true;
                    }
                }
                else if (newWeaponUseState)
                {
                    HudMessageReceiver.Instance.SendHudMessage("Warning: having any UFG weapons enabled will prevent your CyberGrind score from being submitted.");
                }

            }


        }
    }
}