using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(GunControl),"UpdateWeaponList")]
    public static class GunControlPatch
    {
        public static void Postfix(bool firstTime)
        {
            HydraLogger.Log($"GunControl: Set weapons {firstTime}");
            WeaponManager.DeployWeapons(firstTime);
        }

    }
}
