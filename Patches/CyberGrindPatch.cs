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
                if(UFGWeaponManager.WeaponsInUse)
                {
                    Debug.Log("UFG: weapons used. Disqualifying cybergrind score.");
                    return false;
                }

                return true;
            }
        }
    }
}
