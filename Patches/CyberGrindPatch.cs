using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraFunGuns.Patches
{
    public class CheatsPatch
    {
        [HarmonyPatch(typeof(SteamController), nameof(SteamController.SubmitCyberGrindScore))]
        public static class CyberGrindPreventer
        {
            public static bool Prefix()
            {
                Debug.Log("ERM CYBERGWIND?");
                if(UFGWeaponManager.WeaponsInUse)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(EndlessGrid),"OnEnable")]
        public static class CyberWarning
        {
            public static void Postfix()
            {

            }
        }
    }
}
