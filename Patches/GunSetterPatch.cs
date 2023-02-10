using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace UltraFunGuns
{
    [HarmonyPatch(typeof(GunSetter),"ResetWeapons")]
    public static class GunSetterPatch
    {
        public static void Postfix(bool firstTime)
        {
            try
            {
                WeaponManager.DeployWeapons(firstTime);
            }
            catch (System.Exception e)
            {
                HydraLogger.Log("Weapon deployment attempted.");
            }
            
        }

    }
}
